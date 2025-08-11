#!/bin/bash

# Docker Troubleshooting and Fix Script for Hetzner Servers
# Run this script if Docker installation or startup fails

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

log "🔧 Starting Docker troubleshooting..."

# Function to check system information
check_system_info() {
    log "Checking system information..."
    info "OS: $(lsb_release -d | cut -f2)"
    info "Kernel: $(uname -r)"
    info "Architecture: $(dpkg --print-architecture)"
    info "Available memory: $(free -h | awk '/^Mem:/ {print $2}')"
    info "Available disk space: $(df -h / | awk 'NR==2 {print $4}')"
}

# Function to check Docker status
check_docker_status() {
    log "Checking Docker status..."
    
    if command -v docker &> /dev/null; then
        info "Docker is installed"
        docker --version
    else
        error "Docker is not installed"
        return 1
    fi
    
    if systemctl is-active --quiet docker; then
        info "Docker service is running"
    else
        error "Docker service is not running"
        systemctl status docker --no-pager || true
        return 1
    fi
    
    if docker info &> /dev/null; then
        info "Docker daemon is responding"
    else
        error "Docker daemon is not responding"
        return 1
    fi
}

# Function to check for common issues
check_common_issues() {
    log "Checking for common issues..."
    
    # Check if systemd is running
    if ! systemctl --version &> /dev/null; then
        error "systemd is not available"
        return 1
    fi
    
    # Check if cgroups v2 is causing issues
    if [ -f /sys/fs/cgroup/cgroup.controllers ]; then
        warn "System is using cgroups v2, which might cause Docker issues on older systems"
    fi
    
    # Check for conflicting packages
    if dpkg -l | grep -q "containerd.io\|docker.io\|docker-engine"; then
        warn "Found potentially conflicting Docker packages"
        dpkg -l | grep -E "(containerd|docker)" || true
    fi
    
    # Check disk space
    local disk_usage=$(df / | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ "$disk_usage" -gt 90 ]; then
        error "Disk usage is too high: $disk_usage%"
        return 1
    fi
    
    # Check memory
    local mem_available=$(free | awk 'NR==2{printf "%.0f", $7/1024/1024}')
    if [ "$mem_available" -lt 1 ]; then
        warn "Available memory is low: ${mem_available}GB"
    fi
}

# Function to completely remove Docker
remove_docker() {
    log "Completely removing Docker..."
    
    # Stop Docker service
    systemctl stop docker || true
    systemctl stop docker.socket || true
    systemctl stop containerd || true
    
    # Remove Docker packages
    apt remove -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin || true
    apt remove -y docker docker-engine docker.io containerd runc || true
    apt autoremove -y
    
    # Remove Docker directories
    rm -rf /var/lib/docker
    rm -rf /var/lib/containerd
    rm -rf /etc/docker
    rm -rf /var/run/docker.sock
    
    # Remove Docker group
    groupdel docker || true
    
    # Remove repository
    rm -f /etc/apt/sources.list.d/docker.list
    rm -f /usr/share/keyrings/docker-archive-keyring.gpg
    
    log "Docker completely removed"
}

# Function to install Docker using alternative method
install_docker_alternative() {
    log "Installing Docker using alternative method..."
    
    # Update package list
    apt update
    
    # Install prerequisites
    apt install -y apt-transport-https ca-certificates curl gnupg lsb-release
    
    # Use Docker's convenience script
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    rm get-docker.sh
    
    # Install Docker Compose manually
    local compose_version=$(curl -s https://api.github.com/repos/docker/compose/releases/latest | grep 'tag_name' | cut -d\" -f4)
    curl -L "https://github.com/docker/compose/releases/download/${compose_version}/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
    
    log "Alternative Docker installation completed"
}

# Function to fix Docker daemon issues
fix_docker_daemon() {
    log "Fixing Docker daemon configuration..."
    
    # Create Docker configuration directory
    mkdir -p /etc/docker
    
    # Create or update daemon.json with better configuration
    cat > /etc/docker/daemon.json << 'EOF'
{
    "log-driver": "json-file",
    "log-opts": {
        "max-size": "10m",
        "max-file": "3"
    },
    "storage-driver": "overlay2",
    "exec-opts": ["native.cgroupdriver=systemd"],
    "live-restore": true,
    "userland-proxy": false,
    "experimental": false,
    "ip-forward": true,
    "iptables": true,
    "ip-masq": true
}
EOF
    
    # Fix systemd service if needed
    systemctl daemon-reload
    
    log "Docker daemon configuration fixed"
}

# Function to test Docker installation
test_docker() {
    log "Testing Docker installation..."
    
    # Start Docker service
    systemctl start docker
    systemctl enable docker
    
    # Wait for Docker to start
    sleep 5
    
    # Test Docker
    if docker run --rm hello-world > /dev/null 2>&1; then
        log "✅ Docker test passed!"
        return 0
    else
        error "❌ Docker test failed!"
        return 1
    fi
}

# Function to fix permissions
fix_permissions() {
    log "Fixing Docker permissions..."
    
    # Add deploy user to docker group if exists
    if id "deploy" &>/dev/null; then
        usermod -aG docker deploy
        log "Added deploy user to docker group"
    fi
    
    # Fix socket permissions
    if [ -S /var/run/docker.sock ]; then
        chmod 666 /var/run/docker.sock
        log "Fixed docker socket permissions"
    fi
}

# Main troubleshooting function
main() {
    log "🚀 Starting Docker troubleshooting and repair..."
    
    check_system_info
    
    if check_docker_status && check_common_issues; then
        log "✅ Docker appears to be working correctly!"
        return 0
    fi
    
    error "Docker issues detected. Attempting to fix..."
    
    # Try to fix daemon configuration first
    fix_docker_daemon
    
    if systemctl restart docker && test_docker; then
        log "✅ Docker fixed by updating daemon configuration!"
        fix_permissions
        return 0
    fi
    
    # If that doesn't work, try complete reinstall
    warn "Attempting complete Docker reinstall..."
    remove_docker
    sleep 3
    install_docker_alternative
    fix_docker_daemon
    fix_permissions
    
    if test_docker; then
        log "✅ Docker successfully reinstalled and working!"
        return 0
    else
        error "❌ Failed to fix Docker. Manual intervention required."
        error "Please check the following:"
        error "1. System logs: journalctl -xeu docker.service"
        error "2. Docker logs: cat /var/log/docker.log"
        error "3. System resources: free -h && df -h"
        error "4. Kernel version compatibility"
        return 1
    fi
}

# Parse command line arguments
case "${1:-fix}" in
    "check")
        check_system_info
        check_docker_status
        check_common_issues
        ;;
    "fix"|"repair")
        main
        ;;
    "remove")
        remove_docker
        ;;
    "install")
        install_docker_alternative
        fix_docker_daemon
        test_docker
        fix_permissions
        ;;
    "test")
        test_docker
        ;;
    "help"|*)
        echo "Docker Troubleshooting Script"
        echo ""
        echo "Usage: $0 {check|fix|remove|install|test|help}"
        echo ""
        echo "Commands:"
        echo "  check   - Check Docker status and common issues"
        echo "  fix     - Attempt to fix Docker issues (default)"
        echo "  remove  - Completely remove Docker"
        echo "  install - Install Docker using alternative method"
        echo "  test    - Test Docker installation"
        echo "  help    - Show this help"
        ;;
esac
