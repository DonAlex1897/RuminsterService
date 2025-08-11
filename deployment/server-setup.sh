#!/bin/bash

# Server Setup Script for Hetzner Ubuntu Server
# Run this script as root on your new Hetzner server

set -e

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

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    error "This script must be run as root"
    exit 1
fi

log "🚀 Starting Hetzner server setup..."

# Update system
log "Updating system packages..."
apt update && apt upgrade -y

# Install essential packages
log "Installing essential packages..."
apt install -y curl wget git unzip software-properties-common apt-transport-https ca-certificates gnupg lsb-release htop tree vim

# Remove any existing Docker installations
log "Removing any existing Docker installations..."
apt remove -y docker docker-engine docker.io containerd runc 2>/dev/null || true

# Install Docker - Fixed method for Hetzner/Ubuntu
log "Installing Docker..."
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null

apt update

# Install Docker with retry logic
for i in {1..3}; do
    log "Docker installation attempt $i/3..."
    if apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin; then
        log "Docker installed successfully!"
        break
    else
        error "Docker installation attempt $i failed"
        if [ $i -eq 3 ]; then
            error "All Docker installation attempts failed. Trying alternative method..."
            # Alternative installation method
            curl -fsSL https://get.docker.com -o get-docker.sh
            sh get-docker.sh
            rm get-docker.sh
        fi
        sleep 5
    fi
done

# Configure Docker daemon (fix for common Hetzner issues)
log "Configuring Docker daemon..."
mkdir -p /etc/docker
cat > /etc/docker/daemon.json << 'EOF'
{
    "log-driver": "json-file",
    "log-opts": {
        "max-size": "10m",
        "max-file": "3"
    },
    "storage-driver": "overlay2"
}
EOF

# Start and enable Docker with retry
log "Starting Docker service..."
for i in {1..3}; do
    if systemctl start docker && systemctl enable docker; then
        log "Docker started successfully!"
        break
    else
        error "Failed to start Docker, attempt $i/3"
        if [ $i -eq 3 ]; then
            error "Failed to start Docker after 3 attempts"
            log "Checking Docker status..."
            systemctl status docker --no-pager || true
            log "Checking Docker logs..."
            journalctl -xeu docker.service --no-pager -n 20 || true
            exit 1
        fi
        sleep 10
    fi
done

# Install Docker Compose (standalone version as backup)
log "Installing Docker Compose..."
DOCKER_COMPOSE_VERSION=$(curl -s https://api.github.com/repos/docker/compose/releases/latest | grep 'tag_name' | cut -d\" -f4)
curl -L "https://github.com/docker/compose/releases/download/${DOCKER_COMPOSE_VERSION}/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

# Verify Docker installation
log "Verifying Docker installation..."
if docker --version && docker-compose --version; then
    log "Docker verification successful!"
else
    error "Docker verification failed!"
    exit 1
fi

# Install Nginx
log "Installing Nginx..."
apt install -y nginx

# Install Certbot for SSL
log "Installing Certbot for SSL..."
apt install -y certbot python3-certbot-nginx

# Install additional useful tools
log "Installing additional tools..."
apt install -y fail2ban ufw logrotate

# Create deployment user
log "Creating deployment user..."
if ! id "deploy" &>/dev/null; then
    useradd -m -s /bin/bash deploy
    usermod -aG docker deploy
    usermod -aG sudo deploy
    log "Deploy user created successfully!"
else
    log "Deploy user already exists, adding to docker group..."
    usermod -aG docker deploy
fi

# Create application directory
log "Setting up application directory..."
mkdir -p /opt/ruminster
chown deploy:deploy /opt/ruminster

# Setup SSH directory for deploy user
mkdir -p /home/deploy/.ssh
chmod 700 /home/deploy/.ssh
chown deploy:deploy /home/deploy/.ssh

# Setup basic security with fail2ban
log "Configuring fail2ban..."
systemctl enable fail2ban
systemctl start fail2ban

# Setup firewall
log "Configuring firewall..."
ufw --force reset
ufw default deny incoming
ufw default allow outgoing
ufw allow OpenSSH
ufw allow 'Nginx Full'
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable

# Configure automatic security updates
log "Setting up automatic security updates..."
apt install -y unattended-upgrades
echo 'Unattended-Upgrade::Automatic-Reboot "false";' >> /etc/apt/apt.conf.d/50unattended-upgrades

# Set up log rotation for application logs
log "Setting up log rotation..."
cat > /etc/logrotate.d/ruminster << 'EOF'
/opt/ruminster/logs/*.log {
    daily
    missingok
    rotate 14
    compress
    delaycompress
    notifempty
    copytruncate
}
EOF

# Test Docker with hello-world
log "Testing Docker installation..."
if docker run hello-world > /dev/null 2>&1; then
    log "Docker test successful!"
else
    error "Docker test failed!"
    log "Docker status:"
    systemctl status docker --no-pager
    exit 1
fi

# Clean up
log "Cleaning up..."
apt autoremove -y
apt autoclean

log "✅ Server setup completed successfully!"
log ""
log "📋 Setup Summary:"
log "  ✓ System updated"
log "  ✓ Docker installed and tested"
log "  ✓ Docker Compose installed"
log "  ✓ Nginx installed"
log "  ✓ Certbot installed"
log "  ✓ Deploy user created"
log "  ✓ Firewall configured"
log "  ✓ Security tools installed"
log "  ✓ Log rotation configured"
log ""
log "🔄 Next steps:"
log "1. Switch to deploy user: su - deploy"
log "2. Upload your application files to /opt/ruminster"
log "3. Configure environment variables"
log "4. Run the deployment script"
log ""
log "📁 Important directories:"
log "  - Application: /opt/ruminster"
log "  - Nginx config: /etc/nginx/sites-available/"
log "  - SSL certificates: /etc/letsencrypt/"
log "  - Logs: /var/log/"
log ""
warn "Don't forget to:"
warn "- Configure your domain's DNS to point to this server"
warn "- Copy your SSH public key to /home/deploy/.ssh/authorized_keys"
warn "- Update the server regularly with security patches"
