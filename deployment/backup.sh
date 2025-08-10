#!/bin/bash

# Backup and Maintenance Script for Ruminster
# This script handles automated backups and maintenance tasks

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKUP_DIR="/opt/ruminster/backups"
RETENTION_DAYS=${BACKUP_RETENTION_DAYS:-7}
DATE=$(date +%Y%m%d_%H%M%S)

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1"
}

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Database backup function
backup_database() {
    log "Creating database backup..."
    
    local backup_file="$BACKUP_DIR/postgres_backup_$DATE.sql"
    
    # Get database container ID
    local container_id=$(docker ps --filter "name=ruminster-postgres" --format "{{.ID}}")
    
    if [ -z "$container_id" ]; then
        error "PostgreSQL container not found!"
        return 1
    fi
    
    # Create backup
    docker exec "$container_id" pg_dump -U ruminster_user -d ruminster > "$backup_file"
    
    if [ $? -eq 0 ]; then
        # Compress backup
        gzip "$backup_file"
        log "Database backup created: $backup_file.gz"
        
        # Calculate backup size
        local size=$(du -h "$backup_file.gz" | cut -f1)
        log "Backup size: $size"
        
        return 0
    else
        error "Database backup failed!"
        return 1
    fi
}

# Application files backup
backup_app_files() {
    log "Creating application files backup..."
    
    local backup_file="$BACKUP_DIR/app_files_$DATE.tar.gz"
    
    # Backup important files
    tar -czf "$backup_file" \
        -C /opt/ruminster \
        --exclude='backups' \
        --exclude='bin' \
        --exclude='obj' \
        --exclude='.git' \
        --exclude='logs' \
        .
    
    if [ $? -eq 0 ]; then
        log "Application files backup created: $backup_file"
        
        local size=$(du -h "$backup_file" | cut -f1)
        log "Backup size: $size"
        
        return 0
    else
        error "Application files backup failed!"
        return 1
    fi
}

# Docker volumes backup
backup_docker_volumes() {
    log "Creating Docker volumes backup..."
    
    local backup_file="$BACKUP_DIR/docker_volumes_$DATE.tar.gz"
    
    # Stop containers temporarily
    cd /opt/ruminster
    docker-compose -f docker-compose.prod.yml stop
    
    # Backup volumes
    docker run --rm \
        -v ruminster_postgres_data:/data \
        -v "$BACKUP_DIR:/backup" \
        alpine tar -czf "/backup/docker_volumes_$DATE.tar.gz" -C /data .
    
    # Start containers
    docker-compose -f docker-compose.prod.yml start
    
    if [ -f "$backup_file" ]; then
        log "Docker volumes backup created: $backup_file"
        return 0
    else
        error "Docker volumes backup failed!"
        return 1
    fi
}

# Clean old backups
cleanup_old_backups() {
    log "Cleaning up old backups (keeping last $RETENTION_DAYS days)..."
    
    find "$BACKUP_DIR" -name "*.sql.gz" -mtime +$RETENTION_DAYS -delete
    find "$BACKUP_DIR" -name "*.tar.gz" -mtime +$RETENTION_DAYS -delete
    
    local remaining=$(find "$BACKUP_DIR" -type f | wc -l)
    log "Cleanup completed. $remaining backup files remaining."
}

# System maintenance
system_maintenance() {
    log "Performing system maintenance..."
    
    # Update system packages
    sudo apt update && sudo apt upgrade -y
    
    # Clean up Docker
    docker system prune -f
    docker volume prune -f
    
    # Clean up logs
    sudo journalctl --vacuum-time=7d
    
    # Clean up old log files
    find /var/log -name "*.log" -mtime +30 -delete 2>/dev/null || true
    
    log "System maintenance completed!"
}

# Health check
health_check() {
    log "Performing health check..."
    
    local health_url="http://localhost:5000/health"
    local errors=0
    
    # Check if containers are running
    if ! docker-compose -f /opt/ruminster/docker-compose.prod.yml ps | grep -q "Up"; then
        error "Some containers are not running!"
        ((errors++))
    fi
    
    # Check API health endpoint
    if ! curl -f -s "$health_url" > /dev/null; then
        error "API health check failed!"
        ((errors++))
    fi
    
    # Check database connection
    if ! docker exec ruminster-postgres pg_isready -U ruminster_user -d ruminster > /dev/null; then
        error "Database health check failed!"
        ((errors++))
    fi
    
    # Check disk space
    local disk_usage=$(df / | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ "$disk_usage" -gt 80 ]; then
        warn "Disk usage is high: $disk_usage%"
        ((errors++))
    fi
    
    # Check memory usage
    local mem_usage=$(free | awk 'NR==2{printf "%.0f", $3*100/$2}')
    if [ "$mem_usage" -gt 90 ]; then
        warn "Memory usage is high: $mem_usage%"
    fi
    
    if [ $errors -eq 0 ]; then
        log "All health checks passed!"
        return 0
    else
        error "$errors health check(s) failed!"
        return 1
    fi
}

# Restore database from backup
restore_database() {
    log "Restoring database from backup..."
    
    if [ -z "$1" ]; then
        error "Please specify backup file to restore from"
        error "Usage: $0 restore-db /path/to/backup.sql.gz"
        return 1
    fi
    
    local backup_file="$1"
    
    if [ ! -f "$backup_file" ]; then
        error "Backup file not found: $backup_file"
        return 1
    fi
    
    warn "This will replace the current database. Are you sure?"
    read -p "Type 'yes' to continue: " confirm
    
    if [ "$confirm" != "yes" ]; then
        log "Restore cancelled."
        return 0
    fi
    
    # Stop application
    cd /opt/ruminster
    docker-compose -f docker-compose.prod.yml stop app
    
    # Restore database
    if [[ "$backup_file" == *.gz ]]; then
        gunzip -c "$backup_file" | docker exec -i ruminster-postgres psql -U ruminster_user -d ruminster
    else
        docker exec -i ruminster-postgres psql -U ruminster_user -d ruminster < "$backup_file"
    fi
    
    # Start application
    docker-compose -f docker-compose.prod.yml start app
    
    log "Database restore completed!"
}

# Send backup notification (if configured)
send_notification() {
    local status="$1"
    local message="$2"
    
    # This is a placeholder for notification integration
    # You can integrate with services like:
    # - Slack webhook
    # - Discord webhook
    # - Email
    # - Telegram bot
    
    log "Notification: [$status] $message"
}

# Main execution
case "${1:-help}" in
    "full-backup")
        log "Starting full backup..."
        if backup_database && backup_app_files; then
            cleanup_old_backups
            send_notification "SUCCESS" "Full backup completed successfully"
            log "Full backup completed successfully!"
        else
            send_notification "ERROR" "Full backup failed"
            error "Full backup failed!"
            exit 1
        fi
        ;;
    "db-backup")
        backup_database
        cleanup_old_backups
        ;;
    "app-backup")
        backup_app_files
        cleanup_old_backups
        ;;
    "volume-backup")
        backup_docker_volumes
        cleanup_old_backups
        ;;
    "maintenance")
        system_maintenance
        ;;
    "health")
        health_check
        ;;
    "cleanup")
        cleanup_old_backups
        ;;
    "restore-db")
        restore_database "$2"
        ;;
    "daily")
        # Daily maintenance routine
        log "Starting daily maintenance routine..."
        backup_database
        cleanup_old_backups
        health_check
        log "Daily maintenance completed!"
        ;;
    "help"|*)
        echo "Ruminster Backup and Maintenance Script"
        echo ""
        echo "Usage: $0 {full-backup|db-backup|app-backup|volume-backup|maintenance|health|cleanup|restore-db|daily}"
        echo ""
        echo "Commands:"
        echo "  full-backup  - Create complete backup (database + files)"
        echo "  db-backup    - Backup database only"
        echo "  app-backup   - Backup application files only"
        echo "  volume-backup - Backup Docker volumes (requires downtime)"
        echo "  maintenance  - Perform system maintenance"
        echo "  health       - Run health checks"
        echo "  cleanup      - Clean up old backups"
        echo "  restore-db   - Restore database from backup file"
        echo "  daily        - Daily maintenance routine"
        echo "  help         - Show this help"
        echo ""
        echo "Environment Variables:"
        echo "  BACKUP_RETENTION_DAYS - Days to keep backups (default: 7)"
        ;;
esac
