# Role-Based Dashboards - Quick Start Guide

## 🚀 Quick Access

### Main Portal
**http://localhost:8000**

Open this URL and select your role to access the appropriate dashboard.

---

## 👑 Platform Owner Dashboard

**Full System Control - All Services**

### Access
**Direct URL:** http://localhost:8000/dashboard-platform-owner.html

### What You Can Do
- ✅ Complete control over all 13 services
- ✅ Infrastructure management (Kafka, ClickHouse, Redis, PostgreSQL)
- ✅ Create, modify, and delete all resources
- ✅ System configuration and security settings
- ✅ User and role management
- ✅ 9-panel command center view

### Key Features
- **Command Center Tab**: View all 9 services simultaneously in 3x3 grid
- **System Metrics**: Real-time performance indicators
- **Full Access**: No restrictions on any service
- **Purple Theme**: Professional indigo gradient design

### Best For
- System administrators
- DevOps engineers
- Platform architects
- Senior technical leads

---

## ⚙️ Platform Admin Dashboard

**Administrative Access - Core Services**

### Access
**Direct URL:** http://localhost:8000/dashboard-platform-admin.html

### What You Can Do
- ✅ Full access to Grafana, Superset, Metabase
- ✅ Create and manage analytics dashboards
- ✅ Deploy and configure n8n workflows
- ✅ GRC application administration
- ✅ User management (limited)
- ⚠️ View-only for Camunda processes
- 🔒 No access to Kafka or ClickHouse

### Key Features
- **Control Panel Tab**: 4-service grid view
- **Analytics Tools**: Full dashboard creation rights
- **Workflow Management**: Complete n8n access
- **Green Theme**: Clean, professional design
- **Permission Notices**: Clear access level indicators

### Best For
- Operations managers
- Analytics administrators
- Workflow coordinators
- Application administrators

---

## 📊 General Dashboard

**Read-Only Access - Analytics & Monitoring**

### Access
**Direct URL:** http://localhost:8000/unified-dashboard.html

### What You Can Do
- 👁️ View all dashboards and reports
- 👁️ Access analytics visualizations
- 👁️ Monitor system status
- 👁️ See workflows and processes
- 👁️ Grid view for 4 services
- ❌ Cannot create or modify dashboards
- ❌ No administrative functions

### Key Features
- **Tab Navigation**: Easy switching between 9 services
- **Grid View**: Monitor 4 services simultaneously
- **Clean Interface**: Simple, intuitive design
- **Blue Theme**: Professional, accessible layout

### Best For
- Business analysts
- Data analysts
- Report viewers
- General users

---

## 📋 Access Comparison

| Service | Platform Owner | Platform Admin | General User |
|---------|----------------|----------------|--------------|
| Grafana | ✅ Full | ✅ Create/Modify | 👁️ View |
| Superset | ✅ Full | ✅ Create/Modify | 👁️ View |
| Metabase | ✅ Full | ✅ Create/Modify | 👁️ View |
| Kafka UI | ✅ Full | 🔒 Restricted | 👁️ View |
| n8n | ✅ Full | ✅ Full | 👁️ View |
| Camunda | ✅ Full | 👁️ View | 👁️ View |
| ClickHouse | ✅ Full | 🔒 Restricted | 🔒 Restricted |
| GRC MVC | ✅ Full | ✅ Admin | 👁️ View |

**Legend:**
- ✅ Full = Create, Read, Update, Delete
- ✅ Create/Modify = Create, Read, Update (no Delete)
- ✅ Admin = Administrative functions
- 👁️ View = Read-only
- 🔒 Restricted = No access

---

## 🎯 How to Start

### 1. Start the Server

```bash
cd /home/Shahin-ai/Shahin-Jan-2026
./start-dashboard.sh
```

### 2. Open Your Browser

Navigate to: **http://localhost:8000**

### 3. Select Your Role

Click on the dashboard card that matches your role:
- **Platform Owner** - For full system control
- **Platform Admin** - For operational administration
- **General Dashboard** - For viewing and reporting

---

## 🔑 Service Login Credentials

### Grafana (Port 3030)
```
Username: admin
Password: admin2026
```

### Apache Superset (Port 8088)
```
Username: admin
Password: admin2026
```

### GRC MVC Application (Port 8888)
```
Email: Info@doganconsult.com
Password: AhmEma$$123456
```

### ClickHouse (Platform Owner Only, Port 8123)
```
Username: grc_analytics
Password: grc_analytics_2026
URL: http://localhost:8123/play
```

---

## 🎨 Visual Differences

### Platform Owner
- **Color**: Purple/Indigo gradient
- **Badge**: 👑 "PLATFORM OWNER"
- **Grid**: 3x3 (9 panels)
- **Tabs**: 8 specialized tabs
- **Icon**: Crown

### Platform Admin
- **Color**: Green gradient
- **Badge**: ⚙️ "PLATFORM ADMIN"
- **Grid**: 2x2 (4 panels)
- **Tabs**: 6 operational tabs
- **Icon**: Gear

### General User
- **Color**: Blue gradient
- **Badge**: 📊 "GENERAL DASHBOARD"
- **Grid**: 2x2 (4 panels)
- **Tabs**: 9 view tabs
- **Icon**: Chart

---

## 🛠️ Common Tasks

### Platform Owner Tasks
1. **Monitor All Infrastructure**
   - Go to "Command Center" tab
   - View all 9 services in grid

2. **Manage Kafka Streams**
   - Select "Infrastructure" tab
   - Access Kafka UI

3. **Configure ClickHouse**
   - Select "Command Center" tab
   - Click ClickHouse panel

### Platform Admin Tasks
1. **Create Analytics Dashboard**
   - Select "Analytics Tools" tab
   - Choose Superset or Metabase
   - Create new dashboard

2. **Deploy Workflow**
   - Select "Workflows" tab
   - Access n8n interface
   - Create automation

3. **Manage Users**
   - Select "GRC Admin" tab
   - Navigate to user management

### General User Tasks
1. **View Reports**
   - Select "Analytics" tab
   - Browse available dashboards

2. **Monitor Systems**
   - Select "Monitoring" tab
   - View Grafana dashboards

3. **Check Workflows**
   - Select "Workflows" tab
   - View automation status

---

## 🆘 Troubleshooting

### Can't Access Dashboard
```bash
# Check if server is running
ps aux | grep serve-dashboard

# Restart server
cd /home/Shahin-ai/Shahin-Jan-2026
python3 serve-dashboard.py
```

### Service Not Loading
```bash
# Check Docker services
docker-compose ps

# Restart specific service
docker-compose restart <service-name>
```

### Wrong Permissions
- Make sure you're using the correct dashboard URL
- Platform Admin cannot access Kafka/ClickHouse - by design
- General users have read-only access

---

## 📖 Documentation

- **Complete Guide**: [RBAC-DASHBOARDS-README.md](./RBAC-DASHBOARDS-README.md)
- **Service Access**: [DASHBOARD_ACCESS.md](./DASHBOARD_ACCESS.md)
- **General Info**: [DASHBOARD-README.md](./DASHBOARD-README.md)
- **Configuration**: [rbac-config.json](./rbac-config.json)

---

## 🎯 Quick Tips

### For All Users
- Use Grid View to monitor multiple services
- Click external link buttons (⤢) to open in new tab
- Real-time clock shows current system time
- All dashboards auto-refresh

### Platform Owners
- Use Command Center for system-wide overview
- Check infrastructure services regularly
- Review audit logs for security

### Platform Admins
- Focus on operational dashboards
- Manage n8n workflows for automation
- Handle day-to-day user requests

### General Users
- Explore pre-built dashboards
- Use tab navigation for easy switching
- Report issues to Platform Admin

---

## 📞 Support

### Check Status
```bash
# All services status
docker-compose ps

# Specific service logs
docker-compose logs -f <service-name>

# Dashboard server status
netstat -tuln | grep 8000
```

### Common Issues

**Port 8000 in use:**
```bash
# Find process using port
lsof -i :8000

# Change port in serve-dashboard.py
# Edit: PORT = 9000
```

**Service not responding:**
```bash
# Restart service
docker-compose restart <service-name>

# Check logs
docker-compose logs --tail=50 <service-name>
```

---

**Version**: 1.0.0
**Last Updated**: 2026-01-10
**Server Port**: 8000
**Total Dashboards**: 3
**Total Services**: 13
