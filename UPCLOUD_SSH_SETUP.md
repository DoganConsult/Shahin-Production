# UpCloud SSH Key Setup Guide

## 🔐 SSH Keys Generated

**Location:**
- Private Key: `c:\Shahin-ai\shahin_grc_key`
- Public Key: `c:\Shahin-ai\shahin_grc_key.pub`

**Key Details:**
- Type: RSA 4096-bit
- Comment: shahin-grc@upcloud
- Fingerprint: SHA256:BPM5BXZeQlwPXtqjZ/sAQHv9Rjlp2IWNy4D4bBsdnr0

## 📝 Setup Instructions

### Step 1: Copy Public Key to UpCloud

1. Display your public key:
   ```powershell
   Get-Content c:\Shahin-ai\shahin_grc_key.pub
   ```

2. Log in to UpCloud Control Panel

3. Go to **Servers** → Your Server → **SSH Keys** → **Add SSH Key**

4. Paste the public key content

5. Name it: `shahin-grc-key` or similar

### Step 2: Configure Environment Variables

Set these in your PowerShell profile or `.env`:

```powershell
# UpCloud server IP or hostname
$env:UPCLOUD_SSH_HOST = "your-upcloud-server-ip-or-hostname"

# PostgreSQL password (if not using peer authentication)
$env:UPCLOUD_DB_PASSWORD = "your-db-password"

# Optional: SSH user (default is 'root')
$env:UPCLOUD_SSH_USER = "root"
```

### Step 3: Test Connection

```powershell
# Run the test script
.\test-upcloud-db.ps1

# With custom parameters:
.\test-upcloud-db.ps1 -UpCloudHost "grc-postgres.upcloud.internal" `
                      -LocalPort "5433" `
                      -DbUser "postgres"
```

## 🔧 Manual SSH Connection (Alternative)

```powershell
# Start SSH tunnel (keep running in separate terminal)
ssh -i c:\Shahin-ai\shahin_grc_key `
    -L 5433:grc-postgres.upcloud.internal:5432 `
    -N `
    root@your-upcloud-server-ip

# In another terminal, connect to PostgreSQL:
psql -h 127.0.0.1 -p 5433 -U postgres -d GrcMvcDb
```

## ⚠️ Security Best Practices

1. **Keep private key private**
   - Never commit to Git
   - Never share with others
   - Store in secure location

2. **Restrict key permissions** (script does this automatically)
   - Only owner can read private key
   - Remove public permissions

3. **Use key passphrase** (optional, for extra security)
   ```powershell
   # Regenerate with passphrase:
   ssh-keygen -t rsa -b 4096 -f c:\Shahin-ai\shahin_grc_key -C "shahin-grc@upcloud"
   # Enter passphrase when prompted
   ```

4. **Rotate keys periodically**
   - Generate new keys every 6-12 months
   - Remove old public keys from UpCloud

## 🚀 Application Configuration

Update your `.env` or `appsettings.json` to use SSH tunnel:

```env
# When SSH tunnel is active on localhost:5433
ConnectionStrings__DefaultConnection=Host=127.0.0.1;Port=5433;Database=GrcMvcDb;Username=postgres;Password=your-password;
```

## 🧪 Troubleshooting

### SSH connection refused
- Verify UpCloud server IP is correct
- Ensure public key is added to UpCloud server
- Check SSH is enabled on UpCloud server

### PostgreSQL connection timeout
- Ensure SSH tunnel is running: `Get-Process | Where Name -Match ssh`
- Verify database host/port in tunnel command
- Check firewall allows local port 5433

### Permission denied (publickey)
- Regenerate SSH key with proper permissions:
  ```powershell
  ssh-keygen -t rsa -b 4096 -f c:\Shahin-ai\shahin_grc_key
  ```
- Re-upload public key to UpCloud

## 📚 References

- [UpCloud SSH Keys Documentation](https://upcloud.com/help/article/ssh-keys/)
- [SSH Port Forwarding Guide](https://www.ssh.com/ssh/tunneling/example)
- [PostgreSQL Client Connection](https://www.postgresql.org/docs/current/app-psql.html)
