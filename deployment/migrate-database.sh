#!/bin/bash

# Database Migration Script
# This script helps migrate your database from local/current server to Hetzner

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKUP_DIR="$SCRIPT_DIR/backups"
DATE=$(date +%Y%m%d_%H%M%S)

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

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

# Function to backup local database
backup_local_db() {
    log "Creating backup of local database..."
    
    # Prompt for local database connection details
    read -p "Enter local database host (default: localhost): " LOCAL_HOST
    LOCAL_HOST=${LOCAL_HOST:-localhost}
    
    read -p "Enter local database port (default: 5432): " LOCAL_PORT
    LOCAL_PORT=${LOCAL_PORT:-5432}
    
    read -p "Enter local database name: " LOCAL_DB
    read -p "Enter local database username: " LOCAL_USER
    read -s -p "Enter local database password: " LOCAL_PASS
    echo
    
    BACKUP_FILE="$BACKUP_DIR/ruminster_backup_$DATE.sql"
    
    export PGPASSWORD="$LOCAL_PASS"
    pg_dump -h "$LOCAL_HOST" -p "$LOCAL_PORT" -U "$LOCAL_USER" -d "$LOCAL_DB" \
        --no-owner --no-privileges --clean --if-exists > "$BACKUP_FILE"
    
    if [ $? -eq 0 ]; then
        log "Backup created successfully: $BACKUP_FILE"
        # Compress the backup
        gzip "$BACKUP_FILE"
        log "Backup compressed: $BACKUP_FILE.gz"
        return 0
    else
        error "Backup failed!"
        return 1
    fi
}

# Function to restore database on server
restore_remote_db() {
    log "Restoring database on remote server..."
    
    if [ ! -f "$BACKUP_DIR/ruminster_backup_$DATE.sql.gz" ]; then
        error "Backup file not found: $BACKUP_DIR/ruminster_backup_$DATE.sql.gz"
        return 1
    fi
    
    # Decompress backup
    gunzip "$BACKUP_DIR/ruminster_backup_$DATE.sql.gz"
    
    # Restore to remote database (assumes you're running this on the server)
    export PGPASSWORD="$POSTGRES_PASSWORD"
    psql -h localhost -p 5432 -U ruminster_user -d ruminster < "$BACKUP_DIR/ruminster_backup_$DATE.sql"
    
    if [ $? -eq 0 ]; then
        log "Database restored successfully!"
        return 0
    else
        error "Database restore failed!"
        return 1
    fi
}

# Function to run Entity Framework migrations
run_ef_migrations() {
    log "Running Entity Framework migrations..."
    
    # This should be run from your application directory
    cd /opt/ruminster
    
    # Run migrations using the containerized app
    docker-compose -f docker-compose.prod.yml exec app dotnet ef database update
    
    if [ $? -eq 0 ]; then
        log "Migrations completed successfully!"
    else
        warn "Migrations failed or no migrations to apply"
    fi
}

# Main execution
case "${1:-help}" in
    "backup")
        backup_local_db
        ;;
    "restore")
        restore_remote_db
        ;;
    "migrate")
        run_ef_migrations
        ;;
    "full")
        backup_local_db && restore_remote_db && run_ef_migrations
        ;;
    "help"|*)
        echo "Usage: $0 {backup|restore|migrate|full}"
        echo ""
        echo "Commands:"
        echo "  backup  - Create backup of local database"
        echo "  restore - Restore backup to remote database"
        echo "  migrate - Run EF Core migrations"
        echo "  full    - Run complete migration process"
        ;;
esac
