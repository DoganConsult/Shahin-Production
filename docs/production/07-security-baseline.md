# Security Baseline

**Generated:** 2026-01-17  
**Purpose:** Security hardening checklist for production VPS deployment

---

## 1. SSH Hardening

### Disable Password Authentication

Edit `/etc/ssh/sshd_config`:

```bash
# Disable password authentication
PasswordAuthentication no
PubkeyAuthentication yes
PermitRootLogin prohibit-password
ChallengeResponseAuthentication no
UsePAM yes

# Limit authentication attempts
MaxAuthTries 3
LoginGraceTime 60

# Disable empty passwords
PermitEmptyPasswords no

# Use only SSH Protocol 2
Protocol 2

# Idle timeout (5 minutes)
ClientAliveInterval 300
ClientAliveCountMax 0
```

Apply changes:
```bash
sudo systemctl restart sshd
```

### SSH Key Setup

```bash
# On local machine - generate key if needed
ssh-keygen -t ed25519 -C "admin@shahin-ai.com"

# Copy to server
ssh-copy-id -i ~/.ssh/id_ed25519.pub user@your-vps-ip

# Test connection
ssh user@your-vps-ip
```

---

## 2. Firewall Configuration (UFW)

### Basic Rules

```bash
# Reset to defaults
sudo ufw --force reset

# Default policies
sudo ufw default deny incoming
sudo ufw default allow outgoing

# Allow SSH (IMPORTANT: Do this first!)
sudo ufw allow 22/tcp comment 'SSH'

# Allow HTTP/HTTPS
sudo ufw allow 80/tcp comment 'HTTP'
sudo ufw allow 443/tcp comment 'HTTPS'

# Enable firewall
sudo ufw enable

# Verify rules
sudo ufw status verbose
```

### Expected Output

```
Status: active
Logging: on (low)
Default: deny (incoming), allow (outgoing), disabled (routed)

To                         Action      From
--                         ------      ----
22/tcp                     ALLOW IN    Anywhere                   # SSH
80/tcp                     ALLOW IN    Anywhere                   # HTTP
443/tcp                    ALLOW IN    Anywhere                   # HTTPS
22/tcp (v6)                ALLOW IN    Anywhere (v6)              # SSH
80/tcp (v6)                ALLOW IN    Anywhere (v6)              # HTTP
443/tcp (v6)               ALLOW IN    Anywhere (v6)              # HTTPS
```

### What NOT to Expose

| Port | Service | Status |
|------|---------|--------|
| 5432 | PostgreSQL | **BLOCKED** - Internal only |
| 6379 | Redis | **BLOCKED** - Internal only |
| 8080 | API (direct) | **BLOCKED** - Via Nginx only |
| 3000 | Next.js (direct) | **BLOCKED** - Via Nginx only |
| 5678 | n8n | **BLOCKED** - If not needed |
| 8085 | Camunda | **BLOCKED** - If not needed |

---

## 3. Fail2Ban Configuration

### Installation

```bash
sudo apt install fail2ban -y
```

### Configuration

Create `/etc/fail2ban/jail.local`:

```ini
[DEFAULT]
bantime = 3600
findtime = 600
maxretry = 5
banaction = ufw

[sshd]
enabled = true
port = ssh
filter = sshd
logpath = /var/log/auth.log
maxretry = 3
bantime = 86400

[nginx-http-auth]
enabled = true
filter = nginx-http-auth
port = http,https
logpath = /var/log/nginx/grc_portal_error.log
maxretry = 5

[nginx-limit-req]
enabled = true
filter = nginx-limit-req
port = http,https
logpath = /var/log/nginx/grc_portal_error.log
maxretry = 10
```

Enable and start:
```bash
sudo systemctl enable fail2ban
sudo systemctl start fail2ban
sudo fail2ban-client status
```

---

## 4. TLS/SSL Configuration

### Certificate Location

```
/etc/letsencrypt/live/portal.shahin-ai.com/
├── fullchain.pem    # Certificate + intermediates
├── privkey.pem      # Private key
└── chain.pem        # Intermediate certificates
```

### Nginx TLS Settings (from nginx-production.conf)

```nginx
# Modern TLS configuration
ssl_protocols TLSv1.2 TLSv1.3;
ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305';
ssl_prefer_server_ciphers on;
ssl_session_cache shared:SSL:10m;
ssl_session_timeout 10m;
ssl_session_tickets off;

# OCSP Stapling
ssl_stapling on;
ssl_stapling_verify on;
resolver 8.8.8.8 8.8.4.4 valid=300s;
resolver_timeout 5s;
```

### Certificate Renewal

```bash
# Test renewal
sudo certbot renew --dry-run

# Force renewal
sudo certbot renew --force-renewal

# Auto-renewal cron (already configured by certbot)
# Check: cat /etc/cron.d/certbot
```

### Verify TLS Configuration

```bash
# Test with SSL Labs
# Visit: https://www.ssllabs.com/ssltest/analyze.html?d=portal.shahin-ai.com

# Local test
openssl s_client -connect portal.shahin-ai.com:443 -servername portal.shahin-ai.com
```

---

## 5. Security Headers

### Nginx Security Headers (from nginx-production.conf)

```nginx
# HSTS - Force HTTPS for 1 year
add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;

# Prevent clickjacking
add_header X-Frame-Options "SAMEORIGIN" always;

# Prevent MIME type sniffing
add_header X-Content-Type-Options "nosniff" always;

# XSS Protection
add_header X-XSS-Protection "1; mode=block" always;

# Referrer Policy
add_header Referrer-Policy "strict-origin-when-cross-origin" always;

# Permissions Policy
add_header Permissions-Policy "geolocation=(), microphone=(), camera=(), payment=()" always;

# Content Security Policy
add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; img-src 'self' data: https:; font-src 'self' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; connect-src 'self' https:; frame-ancestors 'none';" always;
```

### Verify Headers

```bash
curl -I https://portal.shahin-ai.com | grep -E "(Strict-Transport|X-Frame|X-Content|X-XSS|Referrer-Policy|Content-Security)"
```

---

## 6. Database Security

### PostgreSQL - No Public Access

```yaml
# In docker-compose.prod.yml - NO ports exposed
db:
  image: postgres:15-alpine
  # NO ports: section - internal only
  networks:
    - shahin-prod
```

### PostgreSQL Security Settings

```bash
# Connect to database
docker compose -f infra/compose/docker-compose.prod.yml exec db psql -U grc_admin -d GrcMvcDb

# Verify no public access
\conninfo
# Should show: You are connected to database "GrcMvcDb" as user "grc_admin" via socket

# Check pg_hba.conf (inside container)
docker compose -f infra/compose/docker-compose.prod.yml exec db cat /var/lib/postgresql/data/pg_hba.conf
```

### Redis - No Public Access

```yaml
# In docker-compose.prod.yml - NO ports exposed
redis:
  image: redis:7-alpine
  # NO ports: section - internal only
  command: redis-server --appendonly yes --maxmemory 512mb
  networks:
    - shahin-prod
```

---

## 7. Secrets Management

### Environment Variables

**NEVER commit secrets to git.** Use `.env.production` file:

```bash
# Verify .gitignore includes
cat .gitignore | grep -E "\.env"
# Should show: .env, .env.*, .env.production, etc.
```

### Secret Names (from 03-config-and-secrets.md)

| Secret Name | Purpose | Rotation Frequency |
|-------------|---------|-------------------|
| `DB_PASSWORD` | PostgreSQL password | 90 days |
| `JWT_SECRET` | JWT signing key | 180 days |
| `CLAUDE_API_KEY` | Anthropic API | As needed |
| `AZURE_TENANT_ID` | Azure AD | N/A (ID only) |
| `MSGRAPH_CLIENT_SECRET` | Microsoft Graph | 365 days |
| `SMTP_PASSWORD` | Email sending | 90 days |

### Secret Rotation Procedure

1. **Generate new secret**
2. **Update `.env.production`** on VPS
3. **Restart affected services**:
   ```bash
   docker compose -f infra/compose/docker-compose.prod.yml restart grc-api
   ```
4. **Verify functionality**
5. **Revoke old secret** (if applicable)

---

## 8. Container Security

### Non-Root User (from Dockerfile)

```dockerfile
# Create non-root user for security
RUN groupadd -g 1000 appuser && \
    useradd -m -u 1000 -g appuser appuser && \
    chown -R appuser:appuser /app

USER appuser
```

### Read-Only File Systems (Optional)

```yaml
# In docker-compose.prod.yml
grc-api:
  read_only: true
  tmpfs:
    - /tmp
  volumes:
    - grc_logs:/app/logs
    - grc_keys:/app/keys
```

### Resource Limits

```yaml
# In docker-compose.prod.yml
grc-api:
  deploy:
    resources:
      limits:
        cpus: '2'
        memory: 2G
      reservations:
        cpus: '0.5'
        memory: 512M
```

---

## 9. Application Security

### Rate Limiting (from appsettings.json)

```json
{
  "RateLimiting": {
    "Enabled": true,
    "TrialRegistration": {
      "PermitLimit": 5,
      "WindowMinutes": 10
    },
    "ApiRequests": {
      "PermitLimit": 100,
      "WindowMinutes": 1
    },
    "Login": {
      "PermitLimit": 10,
      "WindowMinutes": 5
    }
  }
}
```

### CORS Configuration

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://www.shahin-ai.com",
      "https://shahin-ai.com",
      "https://portal.shahin-ai.com",
      "https://app.shahin-ai.com"
    ]
  }
}
```

### Input Validation

- FluentValidation enabled for all DTOs
- HTML sanitization via `HtmlSanitizer` package
- SQL injection prevention via Entity Framework Core (parameterized queries)

---

## 10. Update/Patch Policy

### System Updates

```bash
# Weekly security updates
sudo apt update && sudo apt upgrade -y

# Automatic security updates
sudo apt install unattended-upgrades -y
sudo dpkg-reconfigure -plow unattended-upgrades
```

### Docker Updates

```bash
# Update Docker images monthly
docker compose -f infra/compose/docker-compose.prod.yml pull
docker compose -f infra/compose/docker-compose.prod.yml up -d
docker image prune -f
```

### Application Updates

- **Frequency:** As needed (feature releases, security patches)
- **Process:** See `08-release-runbook.md`
- **Testing:** Always test in staging first

---

## 11. Security Checklist

### Initial Setup

- [ ] SSH key authentication only
- [ ] Root login disabled
- [ ] UFW firewall enabled (22, 80, 443 only)
- [ ] Fail2Ban installed and configured
- [ ] SSL/TLS certificates installed
- [ ] Security headers configured in Nginx
- [ ] Database not publicly accessible
- [ ] Redis not publicly accessible
- [ ] Secrets in `.env.production` (not in git)
- [ ] Non-root user in containers

### Ongoing

- [ ] Weekly: Check for system updates
- [ ] Monthly: Review access logs
- [ ] Monthly: Update Docker images
- [ ] Quarterly: Rotate secrets
- [ ] Quarterly: Review firewall rules
- [ ] Annually: Renew SSL certificates (auto)
- [ ] Annually: Security audit

---

## 12. Incident Response

### If Compromised

1. **Isolate:** Disconnect from network if possible
2. **Preserve:** Take snapshots/backups of current state
3. **Investigate:** Review logs for attack vector
4. **Remediate:** Patch vulnerability, rotate all secrets
5. **Restore:** From clean backup if needed
6. **Report:** Document incident and notify stakeholders

### Emergency Contacts

| Role | Contact |
|------|---------|
| System Admin | admin@shahin-ai.com |
| Security Lead | security@shahin-ai.com |
| On-Call | +966-XXX-XXXX |

---

## Next Steps

- [ ] Complete all checklist items
- [ ] Schedule regular security reviews
- [ ] Set up vulnerability scanning (optional)
- [ ] Implement WAF (optional)
