# 🚂 Railway SSH Commands

## Your Service Details

- **Project ID:** `402d90cb-9706-4b98-ae24-0f2e992c624c`
- **Environment ID:** `03604398-8431-4c35-8fce-e230c4c8d585`
- **Service ID:** `0cb7da15-a249-4cba-a197-677e800c306a`

## Database Connection Info

- **Connection URL:** `postgresql://postgres:VUykzDaybssURQkSAfxUYOBKBkDQSuVW@centerbeam.proxy.rlwy.net:11539/railway`
- **Host:** `centerbeam.proxy.rlwy.net`
- **Port:** `11539`
- **Database:** `railway`
- **Username:** `postgres`

---

## 🔧 Quick SSH Commands

### Connect to Service:
```bash
railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```

### Check DATABASE_URL:
```bash
railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a "echo \$DATABASE_URL"
```

### Test PostgreSQL Connection:
```bash
railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a "psql \$DATABASE_URL -c 'SELECT version(), current_database(), current_user;'"
```

### Test with Application:
```bash
railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a "cd /app && dotnet run -- TestDb"
```

### Check Application Logs:
```bash
railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a "tail -n 100 /app/logs/grcmvc-*.log"
```

---

## 📋 Interactive SSH Session

Once connected, you can run:

```bash
# Check environment
echo $DATABASE_URL
printenv | grep -i connection

# Test database
psql $DATABASE_URL -c "SELECT version(), current_database(), current_user;"

# Check application
cd /app
ls -la

# Run test
dotnet run -- TestDb

# Check logs
tail -f /app/logs/grcmvc-*.log
```

---

## ✅ Expected Results

### DATABASE_URL should be:
```
postgresql://postgres:VUykzDaybssURQkSAfxUYOBKBkDQSuVW@centerbeam.proxy.rlwy.net:11539/railway
```

### PostgreSQL Test should show:
```
PostgreSQL 15.x
Database: railway
User: postgres
```

### Application Test should show:
```
[CONFIG] ✅ Converted Railway DATABASE_URL to connection string
[CONFIG] ✅ Connection string format validated
✅ Connection Test Successful!
```

---

**Ready to connect!** 🚂
