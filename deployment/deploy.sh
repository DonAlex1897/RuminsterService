#!/bin/bash

# Ruminster Deployment Script for Hetzner Server
# This script automates the deployment process

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
DEPLOY_DIR="/opt/ruminster"
SERVICE_NAME="ruminster"
DOMAIN="yourdomain.com"  # Change this to your actual domain

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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

info() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')] INFO:${NC} $1"
}

# Check if running as deploy user
check_user() {
    if [ "$USER" != "deploy" ]; then
        error "This script should be run as the 'deploy' user"
        error "Switch to deploy user: sudo su - deploy"
        exit 1
    fi
}

# Setup deployment directory
setup_deploy_dir() {
    log "Setting up deployment directory..."
    
    sudo mkdir -p "$DEPLOY_DIR"
    sudo chown -R deploy:deploy "$DEPLOY_DIR"
    cd "$DEPLOY_DIR"
    
    # Initialize git if not exists
    if [ ! -d ".git" ]; then
        git init
        git remote add origin https://github.com/DonAlex1897/RuminsterService.git
    fi
}

# Pull latest code
deploy_code() {
    log "Deploying latest code..."
    cd "$DEPLOY_DIR"
    
    # Stash any local changes
    git stash || true
    
    # Pull latest changes
    git fetch origin
    git reset --hard origin/master
    
    # Copy environment file if it doesn't exist
    if [ ! -f ".env" ]; then
        cp .env.example .env
        warn "Created .env file from template. Please update it with your production values!"
        warn "Edit: nano $DEPLOY_DIR/.env"
    fi
}

# Build and start services
build_and_start() {
    log "Building and starting services..."
    cd "$DEPLOY_DIR"
    
    # Stop existing services
    docker-compose -f docker-compose.prod.yml down || true
    
    # Build new images
    docker-compose -f docker-compose.prod.yml build --no-cache
    
    # Start services
    docker-compose -f docker-compose.prod.yml up -d
    
    # Wait for services to be healthy
    log "Waiting for services to start..."
    sleep 30
    
    # Check if services are running
    if docker-compose -f docker-compose.prod.yml ps | grep -q "Up"; then
        log "Services started successfully!"
    else
        error "Some services failed to start!"
        docker-compose -f docker-compose.prod.yml logs
        exit 1
    fi
}

# Setup Nginx
setup_nginx() {
    log "Setting up Nginx configuration..."
    
    # Copy nginx config
    sudo cp "$DEPLOY_DIR/deployment/nginx.conf" "/etc/nginx/sites-available/$SERVICE_NAME"
    
    # Update domain in config
    sudo sed -i "s/yourdomain.com/$DOMAIN/g" "/etc/nginx/sites-available/$SERVICE_NAME"
    
    # Enable site
    sudo ln -sf "/etc/nginx/sites-available/$SERVICE_NAME" "/etc/nginx/sites-enabled/$SERVICE_NAME"
    
    # Remove default site
    sudo rm -f /etc/nginx/sites-enabled/default
    
    # Test nginx config
    sudo nginx -t
    
    # Restart nginx
    sudo systemctl restart nginx
    
    log "Nginx configured successfully!"
}

# Setup SSL with Let's Encrypt
setup_ssl() {
    log "Setting up SSL certificate..."
    
    # Request SSL certificate
    sudo certbot --nginx -d "$DOMAIN" -d "www.$DOMAIN" --non-interactive --agree-tos --email "admin@$DOMAIN"
    
    if [ $? -eq 0 ]; then
        log "SSL certificate installed successfully!"
    else
        warn "SSL certificate installation failed. You may need to configure it manually."
    fi
}

# Run database migrations
run_migrations() {
    log "Running database migrations..."
    cd "$DEPLOY_DIR"
    
    # Wait for database to be ready
    docker-compose -f docker-compose.prod.yml exec -T postgres pg_isready -U ruminster_user -d ruminster
    
    # Run EF migrations
    docker-compose -f docker-compose.prod.yml exec -T app dotnet ef database update --no-build
    
    log "Database migrations completed!"
}

# Setup monitoring and health checks
setup_monitoring() {
    log "Setting up monitoring..."
    
    # Create health check script
    cat > /tmp/health-check.sh << 'EOF'
#!/bin/bash
HEALTH_URL="http://localhost:5000/health"
if curl -f -s "$HEALTH_URL" > /dev/null; then
    echo "$(date): Health check passed"
else
    echo "$(date): Health check failed"
    # Restart services if health check fails
    cd /opt/ruminster
    docker-compose -f docker-compose.prod.yml restart app
fi
EOF

    sudo mv /tmp/health-check.sh /usr/local/bin/ruminster-health-check.sh
    sudo chmod +x /usr/local/bin/ruminster-health-check.sh
    
    # Add cron job for health checks
    (crontab -l 2>/dev/null; echo "*/5 * * * * /usr/local/bin/ruminster-health-check.sh >> /var/log/ruminster-health.log 2>&1") | crontab -
    
    log "Health monitoring configured!"
}

# Display status
show_status() {
    log "Deployment Status:"
    echo "===================="
    
    info "Services Status:"
    cd "$DEPLOY_DIR"
    docker-compose -f docker-compose.prod.yml ps
    
    echo ""
    info "Nginx Status:"
    sudo systemctl status nginx --no-pager -l
    
    echo ""
    info "Application Logs (last 20 lines):"
    docker-compose -f docker-compose.prod.yml logs --tail=20 app
    
    echo ""
    info "Access your application at:"
    echo "  🌐 https://$DOMAIN"
    echo "  🔍 Health Check: https://$DOMAIN/health"
    
    if command -v curl >/dev/null 2>&1; then
        echo ""
        info "Testing health endpoint..."
        if curl -f -s "http://localhost:5000/health" > /dev/null; then
            echo "✅ Health check passed!"
        else
            echo "❌ Health check failed!"
        fi
    fi
}

# Main deployment function
main() {
    log "🚀 Starting Ruminster deployment to Hetzner..."
    
    check_user
    setup_deploy_dir
    deploy_code
    
    # Ask for domain
    read -p "Enter your domain name (e.g., yourdomain.com): " input_domain
    if [ -n "$input_domain" ]; then
        DOMAIN="$input_domain"
    fi
    
    build_and_start
    setup_nginx
    
    # Ask about SSL
    read -p "Do you want to setup SSL certificate with Let's Encrypt? (y/n): " setup_ssl_choice
    if [[ $setup_ssl_choice =~ ^[Yy]$ ]]; then
        setup_ssl
    fi
    
    run_migrations
    setup_monitoring
    show_status
    
    log "🎉 Deployment completed successfully!"
    warn "Don't forget to:"
    warn "1. Update your .env file with production values"
    warn "2. Configure your domain's DNS to point to this server"
    warn "3. Test all functionality thoroughly"
}

# Parse command line arguments
case "${1:-deploy}" in
    "deploy")
        main
        ;;
    "update")
        deploy_code
        build_and_start
        run_migrations
        show_status
        ;;
    "status")
        show_status
        ;;
    "logs")
        cd "$DEPLOY_DIR"
        docker-compose -f docker-compose.prod.yml logs -f
        ;;
    "restart")
        cd "$DEPLOY_DIR"
        docker-compose -f docker-compose.prod.yml restart
        show_status
        ;;
    "help"|*)
        echo "Ruminster Deployment Script"
        echo ""
        echo "Usage: $0 {deploy|update|status|logs|restart|help}"
        echo ""
        echo "Commands:"
        echo "  deploy  - Full deployment (default)"
        echo "  update  - Update code and restart services"
        echo "  status  - Show current status"
        echo "  logs    - Show application logs"
        echo "  restart - Restart services"
        echo "  help    - Show this help"
        ;;
esac
