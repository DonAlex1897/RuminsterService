#!/bin/bash

# Quick Docker Fix for Hetzner Server
# Run this if Docker failed to start during initial setup

echo "🔧 Quick Docker Fix for Hetzner Server"

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo "❌ This script must be run as root"
    echo "Run: sudo su - && cd /path/to/script && ./quick-docker-fix.sh"
    exit 1
fi

echo "📋 Diagnosing Docker issue..."

# Check Docker status
if systemctl status docker --no-pager; then
    echo "✅ Docker service status checked"
else
    echo "❌ Docker service has issues"
fi

echo ""
echo "📋 Checking Docker logs..."
journalctl -xeu docker.service --no-pager -n 10

echo ""
echo "🔧 Attempting to fix Docker..."

# Stop Docker services
echo "Stopping Docker services..."
systemctl stop docker || true
systemctl stop docker.socket || true
systemctl stop containerd || true

# Remove problematic Docker daemon configuration
echo "Cleaning Docker configuration..."
rm -f /etc/docker/daemon.json

# Create a minimal Docker configuration
echo "Creating minimal Docker configuration..."
mkdir -p /etc/docker
cat > /etc/docker/daemon.json << 'EOF'
{
    "log-driver": "json-file",
    "log-opts": {
        "max-size": "10m",
        "max-file": "3"
    }
}
EOF

# Fix systemd configuration
echo "Reloading systemd..."
systemctl daemon-reload

# Try to start Docker
echo "Starting Docker..."
if systemctl start docker; then
    echo "✅ Docker started successfully!"
    systemctl enable docker
    
    # Test Docker
    echo "Testing Docker..."
    if docker run --rm hello-world > /dev/null 2>&1; then
        echo "✅ Docker test passed!"
        
        # Add deploy user to docker group if exists
        if id "deploy" &>/dev/null; then
            usermod -aG docker deploy
            echo "✅ Deploy user added to docker group"
        fi
        
        echo ""
        echo "🎉 Docker is now working correctly!"
        echo "You can continue with the deployment process."
        echo ""
        echo "Next steps:"
        echo "1. Switch to deploy user: su - deploy"
        echo "2. Continue with your deployment"
        
    else
        echo "❌ Docker test failed"
        echo "Manual intervention required"
    fi
else
    echo "❌ Failed to start Docker"
    echo ""
    echo "📋 Alternative fix - Complete Docker reinstall:"
    echo "1. Run: apt remove -y docker-ce docker-ce-cli containerd.io"
    echo "2. Run: curl -fsSL https://get.docker.com | sh"
    echo "3. Run: systemctl start docker && systemctl enable docker"
    echo ""
    echo "📋 Check these logs for more details:"
    echo "- systemctl status docker"
    echo "- journalctl -xeu docker.service"
    echo "- dmesg | grep docker"
fi
