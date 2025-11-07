# pgAdmin configuration overrides for GitHub Codespaces
import os

# Critical: Accept the X-Forwarded-Proto header from proxy
# This tells Flask that even though it receives HTTP, the client is using HTTPS
class ReverseProxied:
    def __init__(self, app):
        self.app = app

    def __call__(self, environ, start_response):
        scheme = environ.get('HTTP_X_FORWARDED_PROTO', 'http')
        if scheme:
            environ['wsgi.url_scheme'] = scheme
        return self.app(environ, start_response)

# Session and cookie configuration for proxied environment
SESSION_COOKIE_SECURE = False
SESSION_COOKIE_HTTPONLY = True
SESSION_COOKIE_SAMESITE = 'Lax'
SESSION_COOKIE_NAME = 'pga4_session'
ENHANCED_COOKIE_PROTECTION = False

# Disable CSRF for API calls but keep for forms
WTF_CSRF_CHECK_DEFAULT = False
WTF_CSRF_TIME_LIMIT = None

# Trust proxy headers from GitHub Codespaces
PROXY_X_FOR_COUNT = 1
PROXY_X_PROTO_COUNT = 1
PROXY_X_HOST_COUNT = 1
PROXY_X_PORT_COUNT = 1

# Accept requests that appear to come via HTTP (from internal proxy)
PREFERRED_URL_SCHEME = 'https'

# Session configuration
SECRET_KEY = 'pgadmin4-secret-key-for-session-12345'
CSRF_SESSION_KEY = 'csrf_token'

# Server mode
SERVER_MODE = True

# Disable upgrade checks
UPGRADE_CHECK_ENABLED = False

# Disable Talisman HTTPS enforcement
TALISMAN_ENABLED = False
TALISMAN_CONFIG = {
    'force_https': False,
    'force_https_permanent': False,
    'strict_transport_security': False
}