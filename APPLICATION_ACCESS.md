# ✅ Application Access Guide

## Status: ✅ APPLICATION IS RUNNING

---

## 🌐 Access URLs

### Local Access (from server)
- **HTTP:** `http://localhost:5000`
- **HTTPS:** `https://localhost:5001` ⭐ **RECOMMENDED**
- **Health Check:** `https://localhost:5001/health`

### Remote Access (from other machines)
If accessing from another machine, use the server's IP address:
- **HTTP:** `http://<SERVER_IP>:5000`
- **HTTPS:** `https://<SERVER_IP>:5001`

To find server IP:
```bash
hostname -I
# or
ip addr show | grep "inet " | grep -v "127.0.0.1"
```

---

## 🔐 Login Credentials

### Admin User
- **Email:** `admin@grcsystem.com`
- **Password:** `Admin@123456`
- **Role:** SuperAdmin

### Manager User
- **Email:** `manager@grcsystem.com`
- **Password:** `Manager@123456`
- **Role:** ComplianceManager

---

## ⚠️ Certificate Warning

When accessing HTTPS, you'll see a certificate warning because it's a self-signed certificate. This is normal for development.

**To proceed:**
1. Click "Advanced" or "Show Details"
2. Click "Proceed to localhost (unsafe)" or "Accept Risk and Continue"
3. The application will load normally

---

## 🚀 Quick Start Commands

### Start Application
```bash
cd /home/dogan/grc-system
bash scripts/start-application.sh
```

### Check Status
```bash
# Check if running
ps aux | grep dotnet | grep GrcMvc

# Check ports
ss -tlnp | grep -E ":(5000|5001)"

# Test health
curl -k https://localhost:5001/health
```

### View Logs
```bash
tail -f /tmp/grcmvc-app.log
```

### Stop Application
```bash
pkill -f "dotnet.*GrcMvc"
# or
kill $(cat /tmp/grcmvc.pid)
```

---

## 🔧 Troubleshooting

### ERR_CONNECTION_REFUSED

**Possible causes:**
1. Application not running
2. Firewall blocking ports
3. Wrong IP address

**Solutions:**

1. **Check if application is running:**
   ```bash
   ps aux | grep dotnet | grep GrcMvc
   ```

2. **Check if ports are listening:**
   ```bash
   ss -tlnp | grep -E ":(5000|5001)"
   ```

3. **Restart application:**
   ```bash
   cd /home/dogan/grc-system
   bash scripts/start-application.sh
   ```

4. **Check firewall:**
   ```bash
   sudo ufw status
   # If needed, allow ports:
   sudo ufw allow 5000/tcp
   sudo ufw allow 5001/tcp
   ```

5. **Use correct URL:**
   - From server: `https://localhost:5001`
   - From remote: `https://<SERVER_IP>:5001`

### Application Not Responding

1. **Check logs:**
   ```bash
   tail -100 /tmp/grcmvc-app.log
   ```

2. **Check database connection:**
   ```bash
   sudo -u postgres psql -d GrcMvcDb -c "SELECT version();"
   ```

3. **Restart application:**
   ```bash
   pkill -f "dotnet.*GrcMvc"
   sleep 2
   bash scripts/start-application.sh
   ```

---

## ✅ Verification

### Application is working if:
- ✅ Process is running: `ps aux | grep dotnet | grep GrcMvc`
- ✅ Ports are listening: `ss -tlnp | grep ":5000\|:5001"`
- ✅ Health check responds: `curl -k https://localhost:5001/health`
- ✅ Home page loads: `curl -k https://localhost:5001/ | head -5`

---

## 📝 Current Status

**Last Check:** Application is running and responding

**Access:** `https://localhost:5001`  
**Login:** `admin@grcsystem.com` / `Admin@123456`

---

**✅ APPLICATION IS READY FOR USE**
