# Observability Guide

**Generated:** 2026-01-17  
**Purpose:** Logging, metrics, monitoring, and alerting for production environment

---

## 1. Logging Architecture

### Log Locations

| Service | Log Location | Format |
|---------|--------------|--------|
| **API (ASP.NET)** | `/app/logs/grc-system-*.log` | Serilog JSON |
| **API (Container)** | `docker logs shahin-grc-api` | stdout/stderr |
| **Nginx** | `/var/log/nginx/grc_portal_access.log` | Combined |
| **Nginx Errors** | `/var/log/nginx/grc_portal_error.log` | Error |
| **PostgreSQL** | `docker logs shahin-grc-db` | PostgreSQL native |
| **Redis** | `docker logs shahin-grc-redis` | Redis native |

### Serilog Configuration (from appsettings.json)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Hangfire": "Information"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/grc-system-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Log Viewing Commands

```bash
# Real-time API logs
docker compose -f infra/compose/docker-compose.prod.yml logs -f grc-api

# Last 100 lines with timestamps
docker compose -f infra/compose/docker-compose.prod.yml logs --tail=100 -t grc-api

# Filter for errors only
docker compose -f infra/compose/docker-compose.prod.yml logs grc-api 2>&1 | grep -i error

# Application log files (inside container)
docker compose -f infra/compose/docker-compose.prod.yml exec grc-api \
  tail -f /app/logs/grc-system-$(date +%Y%m%d).log

# Nginx access logs
sudo tail -f /var/log/nginx/grc_portal_access.log

# Nginx error logs
sudo tail -f /var/log/nginx/grc_portal_error.log
```

---

## 2. Health Endpoints

### Available Endpoints

| Endpoint | Purpose | Expected Response |
|----------|---------|-------------------|
| `/health` | Full health check | `200 OK` with JSON status |
| `/health/ready` | Readiness (DB connected) | `200 OK` or `503 Service Unavailable` |
| `/health/live` | Liveness (API running) | `200 OK` |

### Health Check Response Format

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "npgsql": {
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0012345"
    },
    "hangfire": {
      "status": "Healthy",
      "duration": "00:00:00.0001234"
    }
  }
}
```

### Monitoring Health with curl

```bash
# Basic health check
curl -s https://portal.shahin-ai.com/health | jq .

# Check specific status
curl -s https://portal.shahin-ai.com/health | jq -r '.status'

# Automated health check script
#!/bin/bash
HEALTH=$(curl -s -o /dev/null -w "%{http_code}" https://portal.shahin-ai.com/health)
if [ "$HEALTH" != "200" ]; then
  echo "ALERT: Health check failed with status $HEALTH"
  # Send alert (email, Slack, etc.)
fi
```

---

## 3. Metrics to Monitor

### System Metrics

| Metric | Warning Threshold | Critical Threshold | Action |
|--------|-------------------|-------------------|--------|
| **CPU Usage** | > 70% | > 90% | Scale up or optimize |
| **Memory Usage** | > 80% | > 95% | Check for leaks, scale up |
| **Disk Usage** | > 70% | > 90% | Clean logs, expand storage |
| **Disk I/O** | > 80% utilization | > 95% | Optimize queries, add SSD |

### Application Metrics

| Metric | Warning Threshold | Critical Threshold | Action |
|--------|-------------------|-------------------|--------|
| **Response Time (p95)** | > 500ms | > 2000ms | Optimize queries, add caching |
| **Error Rate (5xx)** | > 1% | > 5% | Check logs, fix bugs |
| **Request Rate** | Unusual spike | 10x normal | Check for attacks, scale |
| **Active Connections** | > 80% of max | > 95% of max | Increase pool size |

### Database Metrics

| Metric | Warning Threshold | Critical Threshold | Action |
|--------|-------------------|-------------------|--------|
| **Connection Pool** | > 80% used | > 95% used | Increase pool size |
| **Query Time (avg)** | > 100ms | > 500ms | Add indexes, optimize |
| **Dead Tuples** | > 10% of table | > 20% | Run VACUUM |
| **Replication Lag** | > 1s | > 10s | Check network, replica |

### Redis Metrics

| Metric | Warning Threshold | Critical Threshold | Action |
|--------|-------------------|-------------------|--------|
| **Memory Usage** | > 80% of maxmemory | > 95% | Increase limit, eviction |
| **Hit Rate** | < 90% | < 70% | Review cache strategy |
| **Connected Clients** | > 80% of max | > 95% | Increase limit |

---

## 4. Monitoring Commands

### System Resources

```bash
# CPU and Memory
htop
# or
top -bn1 | head -20

# Disk usage
df -h

# Disk I/O
iostat -x 1 5

# Network connections
ss -tuln
netstat -an | grep ESTABLISHED | wc -l
```

### Docker Container Stats

```bash
# Real-time container stats
docker stats

# Specific containers
docker stats shahin-grc-api shahin-grc-db shahin-grc-redis

# Container resource usage
docker compose -f infra/compose/docker-compose.prod.yml top
```

### PostgreSQL Metrics

```bash
# Active connections
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  psql -U grc_admin -d GrcMvcDb -c "SELECT count(*) FROM pg_stat_activity;"

# Long-running queries
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  psql -U grc_admin -d GrcMvcDb -c "
    SELECT pid, now() - pg_stat_activity.query_start AS duration, query
    FROM pg_stat_activity
    WHERE state = 'active' AND now() - pg_stat_activity.query_start > interval '5 seconds'
    ORDER BY duration DESC;"

# Database size
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  psql -U grc_admin -d GrcMvcDb -c "
    SELECT pg_size_pretty(pg_database_size('GrcMvcDb'));"

# Table sizes
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  psql -U grc_admin -d GrcMvcDb -c "
    SELECT relname, pg_size_pretty(pg_total_relation_size(relid))
    FROM pg_catalog.pg_statio_user_tables
    ORDER BY pg_total_relation_size(relid) DESC
    LIMIT 10;"
```

### Redis Metrics

```bash
# Redis info
docker compose -f infra/compose/docker-compose.prod.yml exec redis redis-cli info

# Memory usage
docker compose -f infra/compose/docker-compose.prod.yml exec redis redis-cli info memory

# Connected clients
docker compose -f infra/compose/docker-compose.prod.yml exec redis redis-cli info clients

# Hit rate
docker compose -f infra/compose/docker-compose.prod.yml exec redis redis-cli info stats | grep keyspace
```

---

## 5. Alerting Setup

### Simple Monitoring Script

Create `/opt/shahin-ai/scripts/monitor.sh`:

```bash
#!/bin/bash
# Simple monitoring script for Shahin AI GRC Platform

ALERT_EMAIL="admin@shahin-ai.com"
LOG_FILE="/var/log/shahin-monitor.log"

log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" >> "$LOG_FILE"
}

send_alert() {
    local subject="$1"
    local message="$2"
    echo "$message" | mail -s "[ALERT] Shahin AI: $subject" "$ALERT_EMAIL"
    log "ALERT: $subject - $message"
}

# Health check
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" https://portal.shahin-ai.com/health)
if [ "$HEALTH_STATUS" != "200" ]; then
    send_alert "Health Check Failed" "API health check returned status $HEALTH_STATUS"
fi

# Disk usage
DISK_USAGE=$(df -h / | awk 'NR==2 {print $5}' | sed 's/%//')
if [ "$DISK_USAGE" -gt 90 ]; then
    send_alert "Disk Usage Critical" "Disk usage is at ${DISK_USAGE}%"
elif [ "$DISK_USAGE" -gt 70 ]; then
    log "WARNING: Disk usage at ${DISK_USAGE}%"
fi

# Memory usage
MEM_USAGE=$(free | awk 'NR==2 {printf "%.0f", $3/$2*100}')
if [ "$MEM_USAGE" -gt 95 ]; then
    send_alert "Memory Usage Critical" "Memory usage is at ${MEM_USAGE}%"
elif [ "$MEM_USAGE" -gt 80 ]; then
    log "WARNING: Memory usage at ${MEM_USAGE}%"
fi

# Container status
for container in shahin-grc-api shahin-grc-db shahin-grc-redis shahin-nginx; do
    STATUS=$(docker inspect -f '{{.State.Running}}' "$container" 2>/dev/null)
    if [ "$STATUS" != "true" ]; then
        send_alert "Container Down" "Container $container is not running"
    fi
done

log "Monitor check completed"
```

### Cron Setup

```bash
# Make script executable
chmod +x /opt/shahin-ai/scripts/monitor.sh

# Add to crontab (run every 5 minutes)
crontab -e
# Add:
*/5 * * * * /opt/shahin-ai/scripts/monitor.sh
```

---

## 6. Log Rotation

### Docker Log Rotation

Add to `/etc/docker/daemon.json`:

```json
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "100m",
    "max-file": "5"
  }
}
```

Restart Docker:
```bash
sudo systemctl restart docker
```

### Nginx Log Rotation

Create `/etc/logrotate.d/nginx-shahin`:

```
/var/log/nginx/grc_*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 0640 www-data adm
    sharedscripts
    postrotate
        [ -f /var/run/nginx.pid ] && kill -USR1 `cat /var/run/nginx.pid`
    endscript
}
```

### Application Log Cleanup

```bash
# Clean logs older than 30 days
find /var/log/grc -name "*.log" -mtime +30 -delete

# Add to crontab
0 3 * * * find /var/log/grc -name "*.log" -mtime +30 -delete
```

---

## 7. Application Insights (Optional)

If using Azure Application Insights, configure in `appsettings.Production.json`:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "${APPLICATIONINSIGHTS_CONNECTION_STRING}",
    "EnableAdaptiveSampling": true,
    "EnablePerformanceCounterCollectionModule": true,
    "EnableRequestTrackingTelemetryModule": true,
    "EnableDependencyTrackingTelemetryModule": true
  }
}
```

---

## 8. Troubleshooting Checklist

### API Not Responding

1. Check container status: `docker ps -a | grep grc-api`
2. Check container logs: `docker logs shahin-grc-api --tail=100`
3. Check health endpoint: `curl -v http://localhost:80/health`
4. Check database connectivity
5. Check memory/CPU usage

### High Response Times

1. Check database query times
2. Check Redis hit rate
3. Check container resource limits
4. Review recent deployments
5. Check for N+1 queries in logs

### 5xx Errors

1. Check application logs for stack traces
2. Check database connection pool
3. Check external service connectivity
4. Review recent code changes
5. Check for memory leaks

---

## Next Steps

- [ ] Set up Grafana dashboards (optional)
- [ ] Configure PagerDuty/Slack alerts (optional)
- [ ] Implement distributed tracing (optional)
- [ ] Set up log aggregation (ELK/Loki) (optional)
