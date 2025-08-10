#!/bin/bash

# Server Setup Script for Hetzner Ubuntu Server
# Run this script as root on your new Hetzner server

set -e

echo "🚀 Starting Hetzner server setup..."

# Update system
apt update && apt upgrade -y

# Install essential packages
apt install -y curl wget git unzip software-properties-common apt-transport-https ca-certificates gnupg lsb-release

# Install Docker
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
apt update
apt install -y docker-ce docker-ce-cli containerd.io

# Install Docker Compose
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

# Start and enable Docker
systemctl start docker
systemctl enable docker

# Install Nginx
apt install -y nginx

# Install Certbot for SSL
apt install -y certbot python3-certbot-nginx

# Create deployment user
useradd -m -s /bin/bash deploy
usermod -aG docker deploy

# Create application directory
mkdir -p /opt/ruminster
chown deploy:deploy /opt/ruminster

# Setup firewall
ufw allow OpenSSH
ufw allow 'Nginx Full'
ufw --force enable

echo "✅ Server setup completed!"
echo "Next steps:"
echo "1. Switch to deploy user: su - deploy"
echo "2. Upload your application files"
echo "3. Configure environment variables"
