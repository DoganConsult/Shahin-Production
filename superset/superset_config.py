# Superset Configuration for GRC System
# Apache License 2.0

import os
from datetime import timedelta

# ---------------------------------------------------------
# Superset specific config
# ---------------------------------------------------------
ROW_LIMIT = 5000
SUPERSET_WEBSERVER_PORT = 8088

# ---------------------------------------------------------
# Flask App Builder configuration
# ---------------------------------------------------------
SECRET_KEY = os.environ.get('SUPERSET_SECRET_KEY', 'grc_superset_secret_key_2026')

# PostgreSQL Database URI
SQLALCHEMY_DATABASE_URI = (
    f"postgresql://{os.environ.get('DATABASE_USER', 'superset')}:"
    f"{os.environ.get('DATABASE_PASSWORD', 'superset')}@"
    f"{os.environ.get('DATABASE_HOST', 'superset-db')}:"
    f"{os.environ.get('DATABASE_PORT', '5432')}/"
    f"{os.environ.get('DATABASE_DB', 'superset')}"
)

# ---------------------------------------------------------
# Redis Configuration (for caching and Celery)
# ---------------------------------------------------------
REDIS_HOST = os.environ.get('REDIS_HOST', 'redis')
REDIS_PORT = os.environ.get('REDIS_PORT', 6379)

CACHE_CONFIG = {
    'CACHE_TYPE': 'RedisCache',
    'CACHE_DEFAULT_TIMEOUT': 300,
    'CACHE_KEY_PREFIX': 'superset_',
    'CACHE_REDIS_HOST': REDIS_HOST,
    'CACHE_REDIS_PORT': REDIS_PORT,
    'CACHE_REDIS_DB': 1,
}

DATA_CACHE_CONFIG = CACHE_CONFIG

# ---------------------------------------------------------
# Feature Flags
# ---------------------------------------------------------
FEATURE_FLAGS = {
    "ENABLE_TEMPLATE_PROCESSING": True,
    "DASHBOARD_NATIVE_FILTERS": True,
    "DASHBOARD_CROSS_FILTERS": True,
    "DASHBOARD_NATIVE_FILTERS_SET": True,
    "ENABLE_EXPLORE_DRAG_AND_DROP": True,
    "EMBEDDED_SUPERSET": True,
    "ALERT_REPORTS": True,
}

# ---------------------------------------------------------
# Security Settings
# ---------------------------------------------------------
PUBLIC_ROLE_LIKE = "Gamma"
SESSION_COOKIE_SAMESITE = "Lax"
SESSION_COOKIE_SECURE = False
SESSION_COOKIE_HTTPONLY = True

# Enable CORS for embedded dashboards
ENABLE_CORS = True
CORS_OPTIONS = {
    'supports_credentials': True,
    'allow_headers': ['*'],
    'resources': ['*'],
    'origins': [
        'http://localhost:3000',
        'http://localhost:5000',
        'https://shahin-ai.com',
        'https://portal.shahin-ai.com',
    ]
}

# ---------------------------------------------------------
# Embedded Dashboard Settings
# ---------------------------------------------------------
GUEST_ROLE_NAME = "Public"
GUEST_TOKEN_JWT_SECRET = os.environ.get('SUPERSET_SECRET_KEY', 'grc_superset_secret_key_2026')
GUEST_TOKEN_JWT_ALGO = "HS256"
GUEST_TOKEN_HEADER_NAME = "X-GuestToken"
GUEST_TOKEN_JWT_EXP_SECONDS = 300

# ---------------------------------------------------------
# GRC Database Connections (Pre-configured)
# ---------------------------------------------------------
# These will be available to connect in Superset
SQLLAB_ASYNC_TIME_LIMIT_SEC = 60 * 60 * 6
SQLLAB_TIMEOUT = 60
SQL_MAX_ROW = 100000

# ---------------------------------------------------------
# Theme Configuration (Arabic RTL Support)
# ---------------------------------------------------------
LANGUAGES = {
    'en': {'flag': 'us', 'name': 'English'},
    'ar': {'flag': 'sa', 'name': 'العربية'},
}
BABEL_DEFAULT_LOCALE = 'ar'

# ---------------------------------------------------------
# Dashboard Refresh
# ---------------------------------------------------------
DASHBOARD_AUTO_REFRESH_MODE = "fetch"
DASHBOARD_VIRTUALIZATION = True

# ---------------------------------------------------------
# Alert and Reports (Email)
# ---------------------------------------------------------
ALERT_REPORTS_NOTIFICATION_DRY_RUN = True
SMTP_HOST = os.environ.get('SMTP_HOST', 'smtp.office365.com')
SMTP_PORT = int(os.environ.get('SMTP_PORT', 587))
SMTP_STARTTLS = True
SMTP_SSL = False
SMTP_USER = os.environ.get('SMTP_USER', 'info@shahin-ai.com')
SMTP_PASSWORD = os.environ.get('SMTP_PASSWORD', '')
SMTP_MAIL_FROM = os.environ.get('SMTP_FROM', 'info@shahin-ai.com')

# ---------------------------------------------------------
# Logging
# ---------------------------------------------------------
LOG_FORMAT = '%(asctime)s:%(levelname)s:%(name)s:%(message)s'
LOG_LEVEL = 'INFO'
ENABLE_TIME_ROTATE = True
TIME_ROTATE_LOG_LEVEL = 'DEBUG'
