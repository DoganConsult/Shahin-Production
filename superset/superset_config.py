# Superset Configuration for GRC Multi-Tenant Analytics
# ============================================================================

import os
from datetime import timedelta

# ============================================================================
# Flask App Builder configuration
# ============================================================================
ROW_LIMIT = 5000
SUPERSET_WEBSERVER_PORT = 8088

# ============================================================================
# Secret key - CHANGE IN PRODUCTION
# ============================================================================
SECRET_KEY = os.environ.get('SUPERSET_SECRET_KEY', 'grc_superset_secret_key_2026')

# ============================================================================
# Database configuration
# ============================================================================
SQLALCHEMY_DATABASE_URI = (
    f"postgresql://{os.environ.get('DATABASE_USER', 'postgres')}:"
    f"{os.environ.get('DATABASE_PASSWORD', 'postgres')}@"
    f"{os.environ.get('DATABASE_HOST', 'db')}:"
    f"{os.environ.get('DATABASE_PORT', '5432')}/"
    f"{os.environ.get('DATABASE_DB', 'superset')}"
)

# ============================================================================
# Redis cache configuration
# ============================================================================
REDIS_HOST = os.environ.get('REDIS_HOST', 'redis')
REDIS_PORT = os.environ.get('REDIS_PORT', '6379')

CACHE_CONFIG = {
    'CACHE_TYPE': 'RedisCache',
    'CACHE_DEFAULT_TIMEOUT': 300,
    'CACHE_KEY_PREFIX': 'superset_',
    'CACHE_REDIS_HOST': REDIS_HOST,
    'CACHE_REDIS_PORT': REDIS_PORT,
    'CACHE_REDIS_DB': 1,
}

DATA_CACHE_CONFIG = {
    'CACHE_TYPE': 'RedisCache',
    'CACHE_DEFAULT_TIMEOUT': 86400,
    'CACHE_KEY_PREFIX': 'superset_data_',
    'CACHE_REDIS_HOST': REDIS_HOST,
    'CACHE_REDIS_PORT': REDIS_PORT,
    'CACHE_REDIS_DB': 2,
}

FILTER_STATE_CACHE_CONFIG = {
    'CACHE_TYPE': 'RedisCache',
    'CACHE_DEFAULT_TIMEOUT': 86400,
    'CACHE_KEY_PREFIX': 'superset_filter_',
    'CACHE_REDIS_HOST': REDIS_HOST,
    'CACHE_REDIS_PORT': REDIS_PORT,
    'CACHE_REDIS_DB': 3,
}

EXPLORE_FORM_DATA_CACHE_CONFIG = {
    'CACHE_TYPE': 'RedisCache',
    'CACHE_DEFAULT_TIMEOUT': 86400,
    'CACHE_KEY_PREFIX': 'superset_explore_',
    'CACHE_REDIS_HOST': REDIS_HOST,
    'CACHE_REDIS_PORT': REDIS_PORT,
    'CACHE_REDIS_DB': 4,
}

# ============================================================================
# Celery configuration for async queries
# ============================================================================
class CeleryConfig:
    broker_url = f'redis://{REDIS_HOST}:{REDIS_PORT}/0'
    imports = ('superset.sql_lab', 'superset.tasks')
    result_backend = f'redis://{REDIS_HOST}:{REDIS_PORT}/0'
    worker_prefetch_multiplier = 1
    task_acks_late = False
    beat_schedule = {
        'reports.scheduler': {
            'task': 'reports.scheduler',
            'schedule': timedelta(minutes=1),
        },
        'reports.prune_log': {
            'task': 'reports.prune_log',
            'schedule': timedelta(days=1),
        },
    }

CELERY_CONFIG = CeleryConfig

# ============================================================================
# Feature flags
# ============================================================================
FEATURE_FLAGS = {
    'ENABLE_TEMPLATE_PROCESSING': True,
    'DASHBOARD_NATIVE_FILTERS': True,
    'DASHBOARD_CROSS_FILTERS': True,
    'DASHBOARD_NATIVE_FILTERS_SET': True,
    'ALERT_REPORTS': True,
    'EMBEDDED_SUPERSET': True,
    'ENABLE_EXPLORE_DRAG_AND_DROP': True,
    'TAGGING_SYSTEM': True,
}

# ============================================================================
# Security - embedding configuration
# ============================================================================
ENABLE_CORS = True
CORS_OPTIONS = {
    'supports_credentials': True,
    'allow_headers': ['*'],
    'resources': ['*'],
    'origins': ['*']
}

# For iframe embedding
HTTP_HEADERS = {
    'X-Frame-Options': 'ALLOWALL'
}
TALISMAN_ENABLED = False

# Guest token for embedding
GUEST_ROLE_NAME = 'Gamma'
GUEST_TOKEN_JWT_SECRET = SECRET_KEY
GUEST_TOKEN_JWT_ALGO = 'HS256'
GUEST_TOKEN_HEADER_NAME = 'X-GuestToken'
GUEST_TOKEN_JWT_EXP_SECONDS = 300

# ============================================================================
# SQL Lab configuration
# ============================================================================
SQLLAB_TIMEOUT = 300
SUPERSET_WEBSERVER_TIMEOUT = 300
SQL_MAX_ROW = 100000
DISPLAY_MAX_ROW = 10000

# ============================================================================
# ClickHouse connection string template
# ============================================================================
# Add this as a database in Superset UI:
# clickhousedb://grc_analytics:grc_analytics_2026@clickhouse:8123/grc_analytics

# ============================================================================
# Logging
# ============================================================================
ENABLE_TIME_ROTATE = True
LOG_LEVEL = 'INFO'
