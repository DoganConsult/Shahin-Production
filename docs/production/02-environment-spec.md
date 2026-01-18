# Environment Specification

**Generated:** 2026-01-16  
**Target:** Single Production VPS running Docker Compose

---

## Operating System

### Recommended
- **OS:** Ubuntu 22.04 LTS (Jammy Jellyfish) or Ubuntu 24.04 LTS (Noble Numbat)
- **Architecture:** x86_64 (amd64)
- **Kernel:** 5.15+ (Ubuntu 22.04) or 6.8+ (Ubuntu 24.04)

### Rationale
- Long-term support (LTS) with security updates until 2027 (22.04) or 2029 (24.04)
- Excellent Docker and Docker Compose support
- Large community and documentation
- Stable package repositories
- Compatible with all required services (.NET 8.0, Node.js, PostgreSQL, Redis)

### Alternative (If Required)
- **Debian 12** (Bookworm) - Similar stability, longer support cycle
- **CentOS Stream 9** / **Rocky Linux 9** - Enterprise-focused, requires different package management

---

## Hardware Requirements

### Minimum Specifications
- **CPU:** 2 vCPU cores
- **RAM:** 4 GB
- **Disk:** 50 GB SSD
- **Network:** 100 Mbps

**Use Case:** Development/testing, low-traffic production (< 100 concurrent users)

### Recommended Specifications
- **CPU:** 4 vCPU cores
- **RAM:** 8 GB
- **Disk:** 100 GB SSD
- **Network:** 1 Gbps

**Use Case:** Production with moderate traffic (100-500 concurrent users)

### Optimal Specifications
- **CPU:** 8 vCPU cores
- **RAM:** 16 GB
- **Disk:** 200 GB SSD (or 100 GB SSD + 100 GB HDD for backups)
- **Network:** 1 Gbps

**Use Case:** High-traffic production (500+ concurrent users), multiple tenants

### Resource Allocation Breakdown

#### Backend API (ASP.NET Core)
- **CPU:** 1-2 cores
- **RAM:** 1-2 GB (base) + 500 MB per 100 concurrent requests
- **Disk:** 10 GB (application + logs)

#### Frontend (Next.js)
- **CPU:** 0.5-1 core
- **RAM:** 500 MB - 1 GB
- **Disk:** 5 GB (build artifacts + cache)

#### Database (PostgreSQL)
- **CPU:** 1-2 cores
- **RAM:** 2-4 GB (base) + 1 GB per 10,000 records
- **Disk:** 20-50 GB (data) + 10-20 GB (WAL logs) + 10 GB (backups)

#### Redis
- **CPU:** 0.5 core
- **RAM:** 500 MB - 1 GB
- **Disk:** 1 GB (persistence files)

#### Nginx
- **CPU:** 0.5 core
- **RAM:** 100-200 MB
- **Disk:** 1 GB (logs + SSL certificates)

#### System Overhead
- **CPU:** 0.5 core
- **RAM:** 1 GB (OS + Docker daemon)
- **Disk:** 10 GB (OS + Docker images)

---

## Required Packages (Docker Approach)

### Base System
```bash
# Update package list
sudo apt update && sudo apt upgrade -y

# Install essential tools
sudo apt install -y curl wget git unzip software-properties-common
```

### Docker & Docker Compose
```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose Plugin (v2)
sudo apt install -y docker-compose-plugin

# Verify installation
docker --version
docker compose version

# Add current user to docker group (optional, for non-sudo usage)
sudo usermod -aG docker $USER
```

**Versions:**
- Docker: 24.0+ (latest stable)
- Docker Compose: 2.20+ (plugin version)

### Firewall (UFW)
```bash
# Install UFW
sudo apt install -y ufw

# Enable firewall
sudo ufw enable

# Allow SSH (CRITICAL - do this first!)
sudo ufw allow 22/tcp

# Allow HTTP/HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Deny everything else by default
sudo ufw default deny incoming
sudo ufw default allow outgoing

# Verify rules
sudo ufw status verbose
```

### Fail2ban (Optional but Recommended)
```bash
# Install Fail2ban
sudo apt install -y fail2ban

# Enable service
sudo systemctl enable fail2ban
sudo systemctl start fail2ban

# Verify
sudo fail2ban-client status
```

### SSL/TLS Certificate Management

#### Option A: Certbot (Let's Encrypt)
```bash
# Install Certbot
sudo apt install -y certbot python3-certbot-nginx

# Or use snap (recommended for latest version)
sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot
```

#### Option B: Traefik (Alternative)
- If using Traefik instead of Nginx, Traefik handles ACME automatically
- No separate certbot installation needed

### Monitoring Tools (Optional)
```bash
# htop (process monitor)
sudo apt install -y htop

# iotop (disk I/O monitor)
sudo apt install -y iotop

# netstat / ss (network tools)
sudo apt install -y net-tools iproute2
```

---

## Firewall Rules (UFW)

### Public Ports (Allowed)
| Port | Protocol | Service | Purpose |
|------|----------|---------|---------|
| 22 | TCP | SSH | Server management |
| 80 | TCP | HTTP | HTTP → HTTPS redirect |
| 443 | TCP | HTTPS | Application access |

### Internal Ports (Denied - Not Exposed)
| Port | Protocol | Service | Purpose |
|------|----------|---------|---------|
| 5432 | TCP | PostgreSQL | Database (internal only) |
| 6379 | TCP | Redis | Cache (internal only) |
| 8080 | TCP | Backend API | Application (internal only) |
| 3000 | TCP | Frontend | Web (internal only) |

### Optional Admin Ports (If Needed, Firewall-Restricted)
| Port | Protocol | Service | Purpose | Restriction |
|------|----------|---------|---------|-------------|
| 5433 | TCP | PostgreSQL | External DB admin | Allow only from specific IPs |
| 6380 | TCP | Redis | External Redis admin | Allow only from specific IPs |

### UFW Configuration Example
```bash
# Basic rules (run in order)
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow 22/tcp comment 'SSH'
sudo ufw allow 80/tcp comment 'HTTP'
sudo ufw allow 443/tcp comment 'HTTPS'

# Optional: Restrict SSH to specific IP (replace with your IP)
# sudo ufw allow from YOUR_IP_ADDRESS to any port 22

# Enable firewall
sudo ufw enable

# Verify
sudo ufw status numbered
```

---

## SSH Hardening

### Baseline Configuration

#### 1. Disable Password Authentication (Keys Only)
Edit `/etc/ssh/sshd_config`:
```ini
PasswordAuthentication no
PubkeyAuthentication yes
AuthorizedKeysFile .ssh/authorized_keys
```

#### 2. Disable Root Login
```ini
PermitRootLogin no
```

#### 3. Change Default Port (Optional)
```ini
Port 2222  # Change from 22 (remember to update UFW rule!)
```

#### 4. Limit Login Attempts
```ini
MaxAuthTries 3
MaxSessions 2
```

#### 5. Disable Unused Features
```ini
X11Forwarding no
AllowTcpForwarding no
```

#### 6. Restart SSH Service
```bash
sudo systemctl restart sshd
sudo systemctl status sshd
```

### SSH Key Setup (Before Disabling Passwords!)

#### Generate Key Pair (On Your Local Machine)
```bash
ssh-keygen -t ed25519 -C "your_email@example.com"
# Save to: ~/.ssh/id_ed25519_shahin_prod
```

#### Copy Public Key to Server
```bash
# Method 1: ssh-copy-id
ssh-copy-id -i ~/.ssh/id_ed25519_shahin_prod.pub user@server_ip

# Method 2: Manual
cat ~/.ssh/id_ed25519_shahin_prod.pub | ssh user@server_ip "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys"
```

#### Test SSH Key Login
```bash
ssh -i ~/.ssh/id_ed25519_shahin_prod user@server_ip
```

#### Verify Key-Only Access Works
```bash
# On server, verify authorized_keys
cat ~/.ssh/authorized_keys

# Test password login (should fail after config change)
ssh -o PreferredAuthentications=password user@server_ip
```

---

## System Updates & Maintenance

### Update Policy
- **Security Updates:** Automatic (via `unattended-upgrades`)
- **Package Updates:** Weekly manual review
- **Kernel Updates:** Require reboot (schedule maintenance window)

### Install Unattended Upgrades
```bash
sudo apt install -y unattended-upgrades

# Configure
sudo dpkg-reconfigure -plow unattended-upgrades

# Verify
sudo systemctl status unattended-upgrades
```

### Update Schedule
```bash
# Weekly update check (add to crontab)
0 2 * * 0 /usr/bin/apt update && /usr/bin/apt list --upgradable
```

---

## Disk Space Management

### Recommended Partition Layout
- **`/` (root):** 20-30 GB (OS + Docker images)
- **`/var/lib/docker`:** 50-100 GB (Docker volumes, images)
- **`/var/log`:** 10 GB (system + application logs)
- **`/var/www/shahin-ai/storage`:** 20-50 GB (file uploads)
- **`/app/backups`:** 20-50 GB (database backups)

### Disk Monitoring
```bash
# Install monitoring
sudo apt install -y ncdu  # Disk usage analyzer

# Check disk usage
df -h

# Check Docker disk usage
docker system df

# Clean Docker (remove unused images, containers, volumes)
docker system prune -a --volumes  # Use with caution!
```

### Log Rotation
```bash
# Install logrotate (usually pre-installed)
sudo apt install -y logrotate

# Configure application log rotation
sudo nano /etc/logrotate.d/grc-app
```

Example `/etc/logrotate.d/grc-app`:
```
/var/log/grc/*.log {
    daily
    rotate 30
    compress
    delaycompress
    missingok
    notifempty
    create 0640 shahin shahin
}
```

---

## Network Configuration

### DNS Resolution
- **Primary DNS:** 8.8.8.8 (Google)
- **Secondary DNS:** 1.1.1.1 (Cloudflare)

Edit `/etc/systemd/resolved.conf`:
```ini
[Resolve]
DNS=8.8.8.8 1.1.1.1
FallbackDNS=8.8.4.4
```

### Time Synchronization
```bash
# Install NTP
sudo apt install -y ntp

# Or use systemd-timesyncd (Ubuntu 22.04+)
sudo systemctl enable systemd-timesyncd
sudo systemctl start systemd-timesyncd

# Verify
timedatectl status
```

---

## Docker Configuration

### Docker Daemon Settings
Edit `/etc/docker/daemon.json`:
```json
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  },
  "storage-driver": "overlay2",
  "default-address-pools": [
    {
      "base": "172.17.0.0/16",
      "size": 24
    }
  ]
}
```

Restart Docker:
```bash
sudo systemctl restart docker
```

### Docker Compose Version
- **Required:** Docker Compose Plugin v2.20+
- **Verify:** `docker compose version`

---

## System Limits

### File Descriptor Limits
Edit `/etc/security/limits.conf`:
```
* soft nofile 65536
* hard nofile 65536
```

### Docker Container Limits
Configure in `docker-compose.prod.yml`:
```yaml
services:
  grc-api:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
```

---

## Security Checklist

- [ ] SSH keys configured and password auth disabled
- [ ] UFW firewall enabled (22/80/443 only)
- [ ] Fail2ban installed and running
- [ ] Unattended security updates enabled
- [ ] Non-root user created for application
- [ ] Docker daemon configured with log rotation
- [ ] Time synchronization enabled
- [ ] Disk space monitoring configured
- [ ] Log rotation configured
- [ ] SSL/TLS certificate automation (certbot) installed

---

## Verification Commands

### System Health
```bash
# CPU and memory
free -h
top

# Disk usage
df -h
du -sh /var/lib/docker

# Network
ip addr show
ss -tulpn

# Docker status
docker ps
docker compose ps
docker system df
```

### Service Status
```bash
# System services
sudo systemctl status docker
sudo systemctl status ssh
sudo systemctl status ufw
sudo systemctl status fail2ban

# Docker services (after deployment)
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs --tail=50
```

---

**Next Step:** Proceed to STEP 4 - Config & Secrets Inventory.
