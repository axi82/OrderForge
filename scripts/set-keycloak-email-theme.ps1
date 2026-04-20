<#
.SYNOPSIS
  Sets the orderforge realm Email theme to orderforge-email (Admin REST / kcadm).

.DESCRIPTION
  Keycloak only imports orderforge-realm.json when the realm does not exist yet. If your realm was
  created earlier, emailTheme in JSON is never applied and password emails stay on the default
  Keycloak template ("Link to reset credentials", subject "Reset password").

  This script updates the live realm using kcadm inside the Keycloak container (preferred), or
  Admin REST from the host if -UseHostApi is specified.

.PARAMETER Container
  Docker container name or id. If omitted, uses the first running container whose name contains "keycloak".

.PARAMETER Realm
  Realm name (default: orderforge).

.PARAMETER EmailTheme
  Email theme folder name (default: orderforge-email).

.PARAMETER AdminUser
  Keycloak master admin username (default: env KEYCLOAK_ADMIN or "admin").

.PARAMETER AdminPassword
  Keycloak master admin password (default: env KEYCLOAK_ADMIN_PASSWORD).

.PARAMETER KeycloakInternalUrl
  URL Keycloak listens on *inside* the container (default: http://127.0.0.1:8080). Used for kcadm.

.PARAMETER UseHostApi
  If set, call Admin REST from PowerShell instead of docker exec (requires KeycloakBaseUrl reachable from host).

.PARAMETER KeycloakBaseUrl
  Base URL when -UseHostApi is set (default: http://127.0.0.1:8080 or env KEYCLOAK_PORT).
#>
[CmdletBinding()]
param(
    [string] $Container,
    [string] $Realm = "orderforge",
    [string] $EmailTheme = "orderforge-email",
    [string] $AdminUser,
    [string] $AdminPassword,
    [string] $KeycloakInternalUrl = "http://127.0.0.1:8080",
    [switch] $UseHostApi,
    [string] $KeycloakBaseUrl
)

$ErrorActionPreference = "Stop"

function Get-DockerKeycloakContainer {
    if ($Container) {
        return $Container
    }
    $ids = docker ps --filter "name=keycloak" -q 2>$null
    if (-not $ids) {
        return $null
    }
    return ($ids -split "`n" | Where-Object { $_ } | Select-Object -First 1).Trim()
}

if (-not $AdminUser) { $AdminUser = $env:KEYCLOAK_ADMIN; if (-not $AdminUser) { $AdminUser = "admin" } }
if (-not $AdminPassword) { $AdminPassword = $env:KEYCLOAK_ADMIN_PASSWORD }
if (-not $AdminPassword) {
    Write-Error "Set KEYCLOAK_ADMIN_PASSWORD or pass -AdminPassword (see docker-compose / .env)."
}

if (-not $UseHostApi) {
    $cid = Get-DockerKeycloakContainer
    if (-not $cid) {
        Write-Error "No running Docker container with name matching 'keycloak'. Start Keycloak (docker compose / Aspire) or pass -Container <id>."
    }

    Write-Host "Using Keycloak container: $cid" -ForegroundColor Cyan
    $kcadm = "/opt/keycloak/bin/kcadm.sh"
    # argv-style docker exec avoids shell quoting issues with passwords (!, $, spaces).
    & docker @(
        "exec", $cid, $kcadm, "config", "credentials",
        "--server", $KeycloakInternalUrl,
        "--realm", "master",
        "--user", $AdminUser,
        "--password", $AdminPassword
    )
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    & docker @(
        "exec", $cid, $kcadm, "update", "realms/$Realm", "-s", "emailTheme=$EmailTheme"
    )
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host "Realm '$Realm' email theme set to '$EmailTheme'. Send another password reset to verify (subject should start with 'Reset your Order Forge')." -ForegroundColor Green
    exit 0
}

# --- Host Admin API (GET realm, merge emailTheme, PUT) ---
if (-not $KeycloakBaseUrl) {
    $p = $env:KEYCLOAK_PORT
    if (-not $p) { $p = "8080" }
    $KeycloakBaseUrl = "http://127.0.0.1:$p"
}
$KeycloakBaseUrl = $KeycloakBaseUrl.TrimEnd("/")

$tokenUri = "$KeycloakBaseUrl/realms/master/protocol/openid-connect/token"
$body = @{
    grant_type    = "password"
    client_id     = "admin-cli"
    username      = $AdminUser
    password      = $AdminPassword
}
$tokenResponse = Invoke-RestMethod -Method Post -Uri $tokenUri -Body $body -ContentType "application/x-www-form-urlencoded"
$accessToken = $tokenResponse.access_token
if (-not $accessToken) { Write-Error "Failed to obtain admin token." }

$headers = @{ Authorization = "Bearer $accessToken" }
$realmUri = "$KeycloakBaseUrl/admin/realms/$Realm"
$realmObj = Invoke-RestMethod -Method Get -Uri $realmUri -Headers $headers
$realmObj.emailTheme = $EmailTheme
$json = $realmObj | ConvertTo-Json -Depth 100
Invoke-RestMethod -Method Put -Uri $realmUri -Headers $headers -Body $json -ContentType "application/json"

Write-Host "Realm '$Realm' email theme set to '$EmailTheme' via Admin API." -ForegroundColor Green
