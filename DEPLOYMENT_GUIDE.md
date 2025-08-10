# 🚀 Ruminster Hetzner Deployment Guide

## Prerequisites Checklist

### Before You Start
- [ ] Hetzner Cloud account created
- [ ] Domain name registered and DNS configured
- [ ] Local database backup created
- [ ] All sensitive data noted (passwords, API keys, etc.)

## 📋 Step-by-Step Deployment

### 1. Create Hetzner Server

1. **Log into Hetzner Cloud Console**
2. **Create new server:**
   - OS: Ubuntu 22.04 LTS
   - Type: CPX21 (2 vCPU, 4GB RAM) minimum
   - Location: Choose closest to your users
   - SSH Key: Add your public key
   - Backups: Enable (recommended)

### 2. Initial Server Setup

```bash
# SSH into your server
ssh root@YOUR_SERVER_IP

# Upload and run the setup script
curl -O https://raw.githubusercontent.com/your-repo/deployment/server-setup.sh
chmod +x server-setup.sh
./server-setup.sh

# Switch to deploy user
su - deploy
```

### 3. Upload Your Project

```bash
# Option A: Git clone (recommended)
cd /opt/ruminster
git clone https://github.com/DonAlex1897/RuminsterService.git .

# Option B: Upload via SCP from your local machine
# From your local machine:
scp -r c:\Projects\ruminster\RuminsterService\ deploy@YOUR_SERVER_IP:/opt/ruminster/
```

### 4. Configure Environment Variables

```bash
cd /opt/ruminster
cp .env.example .env
nano .env
```

**Update these critical values in .env:**
```bash
# Database
POSTGRES_PASSWORD=your_very_secure_password_here

# JWT (generate a secure 32+ character key)
JWT_SECRET_KEY=your_jwt_secret_key_minimum_32_characters_long_and_secure
JWT_ISSUER=https://yourdomain.com
JWT_AUDIENCE=https://yourdomain.com

# Email Configuration
SMTP_HOST=smtp.gmail.com  # or your SMTP provider
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_EMAIL=noreply@yourdomain.com
FROM_NAME=Ruminster

# Application
BASE_URL=https://yourdomain.com
FRONTEND_URL=https://yourdomain.com  # Your frontend URL
```

### 5. Deploy the Application

```bash
cd /opt/ruminster/deployment
chmod +x deploy.sh
./deploy.sh deploy
```

**Follow the prompts:**
- Enter your domain name
- Choose whether to setup SSL (recommended: yes)

### 6. Migrate Your Database

#### Option A: Fresh Installation
If this is a new installation, the migrations will run automatically.

#### Option B: Migrate Existing Data
```bash
# On your local machine, create a backup
cd deployment
chmod +x migrate-database.sh
./migrate-database.sh backup

# Upload backup to server
scp backups/ruminster_backup_*.sql.gz deploy@YOUR_SERVER_IP:/opt/ruminster/backups/

# On server, restore the backup
cd /opt/ruminster/deployment
./migrate-database.sh restore
./migrate-database.sh migrate
```

### 7. Configure Domain DNS

Update your domain's DNS settings:
```
Type: A
Name: @
Value: YOUR_SERVER_IP
TTL: 300

Type: A  
Name: www
Value: YOUR_SERVER_IP
TTL: 300

Type: CNAME
Name: api
Value: yourdomain.com
TTL: 300
```

### 8. Setup Monitoring and Backups

```bash
# Setup automated backups
cd /opt/ruminster/deployment
chmod +x backup.sh

# Add daily backup cron job
crontab -e

# Add these lines:
# Daily backup at 2 AM
0 2 * * * /opt/ruminster/deployment/backup.sh daily >> /var/log/ruminster-backup.log 2>&1

# Weekly full backup on Sunday at 3 AM  
0 3 * * 0 /opt/ruminster/deployment/backup.sh full-backup >> /var/log/ruminster-backup.log 2>&1
```

## 🔧 Post-Deployment Tasks

### 1. Security Hardening

```bash
# Update SSH configuration
sudo nano /etc/ssh/sshd_config

# Recommended changes:
# PermitRootLogin no
# PasswordAuthentication no
# Port 2222  # Change from default 22

sudo systemctl restart ssh

# Setup fail2ban
sudo apt install fail2ban
sudo systemctl enable fail2ban
sudo systemctl start fail2ban
```

### 2. Monitoring Setup

```bash
# Check application status
/opt/ruminster/deployment/deploy.sh status

# View logs
/opt/ruminster/deployment/deploy.sh logs

# Health check
/opt/ruminster/deployment/backup.sh health
```

### 3. SSL Certificate Auto-Renewal

```bash
# Test auto-renewal
sudo certbot renew --dry-run

# The renewal should be automatic, but verify with:
sudo crontab -l | grep certbot
```

## 🚨 Troubleshooting

### Common Issues

#### 1. Application Not Starting
```bash
# Check container logs
cd /opt/ruminster
docker-compose -f docker-compose.prod.yml logs app

# Common fixes:
# - Check .env file values
# - Verify database connection
# - Check port conflicts
```

#### 2. Database Connection Issues
```bash
# Check PostgreSQL container
docker-compose -f docker-compose.prod.yml logs postgres

# Test database connection
docker-compose -f docker-compose.prod.yml exec postgres pg_isready -U ruminster_user -d ruminster
```

#### 3. SSL Issues
```bash
# Check certificate status
sudo certbot certificates

# Renew certificate manually
sudo certbot renew

# Check Nginx configuration
sudo nginx -t
```

#### 4. High Memory Usage
```bash
# Check memory usage
free -h
docker stats

# Restart services if needed
cd /opt/ruminster
docker-compose -f docker-compose.prod.yml restart
```

## 📊 Monitoring Commands

```bash
# Application status
/opt/ruminster/deployment/deploy.sh status

# System resources
htop
df -h
free -h

# Docker containers
docker ps
docker stats

# Application logs
docker-compose -f /opt/ruminster/docker-compose.prod.yml logs -f app

# Nginx logs
sudo tail -f /var/log/nginx/ruminster_access.log
sudo tail -f /var/log/nginx/ruminster_error.log
```

## 🔄 Update and Maintenance

### Update Application
```bash
cd /opt/ruminster/deployment
./deploy.sh update
```

### Manual Restart
```bash
cd /opt/ruminster/deployment  
./deploy.sh restart
```

### Create Manual Backup
```bash
cd /opt/ruminster/deployment
./backup.sh full-backup
```

### System Maintenance
```bash
cd /opt/ruminster/deployment
./backup.sh maintenance
```

## 📈 Performance Optimization

### Database Optimization
```sql
-- Connect to PostgreSQL and run these optimizations
-- docker-compose exec postgres psql -U ruminster_user -d ruminster

-- Analyze tables
ANALYZE;

-- Update statistics
VACUUM ANALYZE;
```

### Nginx Optimization
Already configured in the provided nginx.conf with:
- Gzip compression
- Rate limiting
- Security headers
- Caching for static files

## 🔐 Security Best Practices

1. **Regular Updates**
   - Keep OS packages updated
   - Update Docker images regularly
   - Monitor security advisories

2. **Backup Strategy**
   - Daily database backups
   - Weekly full backups
   - Test restore procedures

3. **Monitoring**
   - Set up log monitoring
   - Monitor disk space
   - Monitor application performance

4. **Access Control**
   - Use SSH keys only
   - Disable root login
   - Use non-standard SSH port
   - Regular security audits

## 📞 Support and Maintenance

### Log Locations
- Application logs: `/opt/ruminster/logs/`
- Nginx logs: `/var/log/nginx/`
- System logs: `/var/log/syslog`
- Backup logs: `/var/log/ruminster-backup.log`

### Important Files
- Environment: `/opt/ruminster/.env`
- Docker Compose: `/opt/ruminster/docker-compose.prod.yml`
- Nginx Config: `/etc/nginx/sites-available/ruminster`
- SSL Certificates: `/etc/letsencrypt/live/yourdomain.com/`

### Emergency Procedures
```bash
# Emergency stop
cd /opt/ruminster
docker-compose -f docker-compose.prod.yml down

# Emergency start
docker-compose -f docker-compose.prod.yml up -d

# Reset to clean state
docker-compose -f docker-compose.prod.yml down -v
docker system prune -af
./deployment/deploy.sh deploy
```

**🎉 Congratulations! Your Ruminster application should now be running on Hetzner Cloud!**

Access your application at: `https://yourdomain.com`
