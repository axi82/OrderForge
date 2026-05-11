#!/bin/sh
set -eu

CONFIG_PATH="/usr/share/nginx/html/appsettings.json"

cat > "$CONFIG_PATH" <<EOF
{
  "ApiBaseUrl": "${API_BASE_URL:-http://localhost:8080/}",
  "Oidc": {
    "Authority": "${OIDC_AUTHORITY:-http://localhost:8081/realms/orderforge}",
    "ClientId": "${OIDC_CLIENT_ID:-orderforge-blazor}"
  },
  "Keycloak": {
    "AuthServerUrl": "${KEYCLOAK_AUTH_SERVER_URL:-http://localhost:8081}",
    "Realm": "${KEYCLOAK_REALM:-orderforge}",
    "ForgotPasswordClientId": "${KEYCLOAK_FORGOT_PASSWORD_CLIENT_ID:-orderforge-blazor}"
  }
}
EOF

exec nginx -g 'daemon off;'
