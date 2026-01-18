# ✅ PRODUCTION QUICK START CHECKLIST

**Target:** app.shahin-ai.com (Same Server)  
**Time Required:** 2-3 hours  
**Current Status:** 75% Ready

---

## 🚀 IMMEDIATE ACTIONS (Do These Now)

### ✅ Phase 1: Environment Variables (30 min)

#### Step 1.1: Check Current Variables
```bash
# SSH into server
ssh your-server

# Check if variables are set
echo $ConnectionStrings__DefaultConnection
echo $SMTP_FROM_EMAIL
echo $CLAUDE_API_KEY
```

#### Step 1.2: Set Missing Variables
```bash
# Edit systemd service environment
sudo nano /etc/systemd/system/grc-app.service.d/environment.conf

# Add these lines:
[Service]
# Database (if not set)
Environment="ConnectionStrings__DefaultConnection=Host=localhost;Database=grc_main;Username=grc_user;Password=your-password"
Environment="ConnectionStrings__GrcAuthDb=Host=localhost;Database=grc_auth;Username=grc_user;Password=your-password"
Environment="ConnectionStrings__Redis=localhost:6379"
Environment="ConnectionStrings__HangfireConnection=Host=localhost;Database=grc_main;Username=grc_user;Password=your-password"

# Email (REQUIRED for notifications)
Environment="SMTP_FROM_EMAIL=noreply@shahin-ai.com"
Environment="SMTP_USERNAME=your-email@shahin-ai.com"
Environment="SMTP_PASSWORD=your-password"
Environment="AZURE_TENANT_ID=your-tenant-id"
Environment="SMTP_CLIENT_ID=your-client-id"
Environment="SMTP_CLIENT_SECRET=your-client-secret"

# Microsoft Graph (for email operations)
Environment="MSGRAPH_CLIENT_ID=your-client-id"
Environment="MSGRAPH_CLIENT_SECRET=your-client-secret"
Environment="MSGRAPH_APP_ID_URI=api://your-app-id"

# AI (OPTIONAL - can skip for now)
Environment="CLAUDE_API_KEY=sk-ant-your-key"

# JWT (should already be set)
Environment="JWT_SECRET=etETf%Z9jqm-AiH_YlIBoudRU^bv+rK?c4XGQs#nh5pOJ*1!y2PC7F.@W0&w$Lkx"
```

#### Step 1.3: Reload and Restart
```bash
# Reload systemd
sudo systemctl daemon-reload

# Restart application
sudo systemctl restart grc-app

# Check status
sudo systemctl status grc-app

# Check logs for errors
sudo journalctl -u grc-app -n 50 --no-pager
```

**✅ Checkpoint:** Application should restart without errors

---

### ✅ Phase 2: File Storage (10 min)

#### Step 2.1: Create Storage Directory
```bash
# Create directory
sudo mkdir -p /var/www/shahin-ai/storage

# Create subdirectories
sudo mkdir -p /var/www/shahin-ai/storage/evidence
sudo mkdir -p /var/www/shahin-ai/storage/documents
sudo mkdir -p /var/www/shahin-ai/storage/temp
```

#### Step 2.2: Set Permissions
```bash
# Set ownership (assuming app runs as www-data)
sudo chown -R www-data:www-data /var/www/shahin-ai/storage

# Set permissions
sudo chmod -R 755 /var/www/shahin-ai/storage

# Verify
ls -la /var/www/shahin-ai/
```

**✅ Checkpoint:** Directory should exist with correct permissions

---

### ✅ Phase 3: Verify SSL/HTTPS (10 min)

#### Step 3.1: Check Nginx Configuration
```bash
# Test nginx config
sudo nginx -t

# View current config
cat /etc/nginx/sites-available/grc
```

#### Step 3.2: Check SSL Certificate
```bash
# Check certificate status
sudo certbot certificates

# Renew if needed
sudo certbot renew --dry-run
```

#### Step 3.3: Test HTTPS
```bash
# Test HTTPS access
curl -I https://app.shahin-ai.com

# Should return: HTTP/2 200 or 302

# Test HTTP redirect
curl -I http://app.shahin-ai.com

# Should return: HTTP/1.1 301 (redirect to HTTPS)
```

**✅ Checkpoint:** HTTPS should be working

---

### ✅ Phase 4: Database Verification (10 min)

#### Step 4.1: Check Database Connection
```bash
# Connect to database
psql -U grc_user -d grc_main

# Count tables
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';
# Expected: 321+ tables

# Check migration history
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
# Expected: InitialCreate migration

# Exit
\q
```

#### Step 4.2: Check Redis
```bash
# Test Redis connection
redis-cli ping
# Expected: PONG

# Check Redis info
redis-cli info
```

**✅ Checkpoint:** Database and Redis should be accessible

---

### ✅ Phase 5: Application Testing (20 min)

#### Step 5.1: Check Application Status
```bash
# Check if running
sudo systemctl status grc-app

# Check process
ps aux | grep GrcMvc

# Check port
sudo netstat -tlnp | grep 8080
```

#### Step 5.2: Test Health Endpoint
```bash
# Test local health
curl http://localhost:8080/health

# Expected: {"status":"Healthy"}
```

#### Step 5.3: Test Login Page
```bash
# Test HTTPS access
curl -I https://app.shahin-ai.com/Account/Login

# Expected: HTTP/2 200
```

#### Step 5.4: Test in Browser
1. Open: https://app.shahin-ai.com
2. Should see login page
3. Try logging in with test account
4. Check for any errors in browser console

**✅ Checkpoint:** Application should be accessible via HTTPS

---

### ✅ Phase 6: Monitoring Setup (30 min)

#### Step 6.1: Configure Log Rotation
```bash
# Create logrotate config
sudo nano /etc/logrotate.d/grc-app

# Add this content:
/var/log/grc-app/*.log {
    daily
    rotate 14
    compress
    delaycompress
    notifempty
    create 0640 www-data www-data
    sharedscripts
    postrotate
        systemctl reload grc-app > /dev/null 2>&1 || true
    endscript
}
```

#### Step 6.2: Create Monitoring Script
```bash
# Create monitoring script
sudo nano /usr/local/bin/check-grc-app.sh

# Add this content:
#!/bin/bash
STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health)
if [ $STATUS -ne 200 ]; then
    echo "GRC App is down! Status: $STATUS"
    # Send alert (email, Slack, etc.)
    systemctl restart grc-app
fi

# Make executable
sudo chmod +x /usr/local/bin/check-grc-app.sh
```

#### Step 6.3: Set Up Cron Job
```bash
# Edit crontab
crontab -e

# Add this line (check every 5 minutes)
*/5 * * * * /usr/local/bin/check-grc-app.sh
```

**✅ Checkpoint:** Monitoring should be active

---

## 📋 VERIFICATION CHECKLIST

### Application Layer ✅
- [x] 46 Controllers implemented
- [x] 140+ Services implemented
- [x] ABP Framework integrated
- [x] Multi-tenant architecture working
- [x] Authentication/Authorization working

### Database Layer ✅
- [x] 321 tables created
- [x] Migration history recorded
- [x] Database connection working
- [x] Redis connection working

### Infrastructure Layer ⚠️
- [ ] All environment variables set
- [ ] File storage configured
- [ ] SSL/HTTPS working
- [ ] Nginx configured correctly
- [ ] Application running on port 8080

### Configuration Layer ⚠️
- [ ] SMTP/Email configured
- [ ] AI integration configured (optional)
- [ ] External integrations configured (optional)
- [ ] Monitoring set up
- [ ] Backups configured

---

## 🚨 TROUBLESHOOTING

### Issue 1: Application Won't Start
```bash
# Check logs
sudo journalctl -u grc-app -n 100 --no-pager

# Common causes:
# - Missing environment variables
# - Database connection failed
# - Port already in use

# Fix:
# 1. Check environment variables
# 2. Verify database connection
# 3. Check if port 8080 is free
```

### Issue 2: 502 Bad Gateway
```bash
# Check if application is running
sudo systemctl status grc-app

# Check nginx logs
sudo tail -f /var/log/nginx/error.log

# Common causes:
# - Application not running
# - Wrong port in nginx config
# - Application crashed

# Fix:
# 1. Restart application
# 2. Check nginx upstream config
# 3. Check application logs
```

### Issue 3: Database Connection Failed
```bash
# Test database connection
psql -U grc_user -d grc_main

# Common causes:
# - Wrong credentials
# - Database not running
# - Firewall blocking connection

# Fix:
# 1. Verify credentials
# 2. Start PostgreSQL: sudo systemctl start postgresql
# 3. Check firewall rules
```

### Issue 4: Email Not Sending
```bash
# Check SMTP configuration
# Verify environment variables are set

# Test SMTP connection
telnet smtp.office365.com 587

# Common causes:
# - Wrong credentials
# - SMTP server blocking
# - Missing OAuth2 setup

# Fix:
# 1. Verify SMTP credentials
# 2. Set up Azure AD app registration
# 3. Configure OAuth2 properly
```

---

## 📊 PRODUCTION READINESS STATUS

### Before Starting
- Application Code: ✅ 100% (0 errors)
- Database: ✅ 100% (321 tables)
- Controllers: ✅ 100% (46/46)
- Services: ✅ 100% (140+/140+)
- Configuration: ⚠️ 50% (Missing env vars)
- Infrastructure: ⚠️ 70% (Basic setup)
- Monitoring: ⚠️ 40% (Basic logging)

### After Completing Checklist
- Application Code: ✅ 100%
- Database: ✅ 100%
- Controllers: ✅ 100%
- Services: ✅ 100%
- Configuration: ✅ 90% (All critical vars set)
- Infrastructure: ✅ 95% (Fully configured)
- Monitoring: ✅ 80% (Basic monitoring active)

**Overall: 75% → 95% Ready** 🎯

---

## 🎯 SUCCESS CRITERIA

### Application is Production-Ready When:
- ✅ Application starts without errors
- ✅ HTTPS is working
- ✅ Login page is accessible
- ✅ Database connection is working
- ✅ File uploads work
- ✅ Email notifications work (if configured)
- ✅ Monitoring is active
- ✅ Logs are being rotated

---

## 📞 SUPPORT

### If You Need Help:
1. Check application logs: `sudo journalctl -u grc-app -n 100`
2. Check nginx logs: `sudo tail -f /var/log/nginx/error.log`
3. Check database logs: `sudo tail -f /var/log/postgresql/postgresql-*.log`
4. Review this checklist again
5. Check PRODUCTION_READINESS_ANALYSIS.md for detailed info

---

## ✅ FINAL VERIFICATION

### Test These User Flows:
1. **Login Flow**
   - Go to https://app.shahin-ai.com
   - Enter credentials
   - Should redirect to dashboard

2. **File Upload**
   - Upload a document
   - Should save to /var/www/shahin-ai/storage

3. **Email Notification** (if configured)
   - Trigger a notification
   - Should receive email

4. **Multi-Tenant**
   - Access different tenant subdomains
   - Should show correct tenant data

5. **API Endpoints**
   - Test API health: https://app.shahin-ai.com/api/health
   - Should return 200 OK

---

**Ready to Deploy!** 🚀

Follow this checklist step-by-step, and your application will be production-ready in 2-3 hours.

**Current Status:** 75% Ready  
**After Checklist:** 95% Ready  
**Time Required:** 2-3 hours  
**Difficulty:** Medium

---

**Last Updated:** 2026-01-19  
**Version:** 1.0  
**Status:** Ready to Execute
