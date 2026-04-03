<#
.SYNOPSIS
  Removes the persistent Docker volume for local Keycloak so the next Aspire/AppHost start re-imports Keycloak/orderforge-realm.json.

.DESCRIPTION
  Keycloak skips startup import when the realm already exists in its database. Deleting this volume forces a clean import
  from the committed realm file. Stop OrderForge.AppHost (and any Keycloak container) before running.

  If removal fails, list volumes: docker volume ls
  Aspire/Docker may prefix names; look for *keycloak* and pass -VolumeName.

.PARAMETER VolumeName
  Docker volume name (default: orderforge-keycloak-data, matching WithDataVolume in OrderForge.AppHost/Program.cs).

.PARAMETER Force
  Skip confirmation prompt.
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string] $VolumeName = "orderforge-keycloak-data",
    [switch] $Force
)

$ErrorActionPreference = "Stop"

Write-Host "This will delete Docker volume '$VolumeName' and all Keycloak data in it (realms, users in that store)." -ForegroundColor Yellow
Write-Host "Stop the AppHost / Keycloak container first." -ForegroundColor Yellow

if (-not $Force -and -not $PSCmdlet.ShouldProcess($VolumeName, "Remove Docker volume")) {
    Write-Host "Aborted."
    exit 0
}

docker volume rm $VolumeName 2>&1 | ForEach-Object { Write-Host $_ }

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "If the volume name differs, run: docker volume ls" -ForegroundColor Cyan
    Write-Host "Then: docker volume rm <name>" -ForegroundColor Cyan
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Volume removed. Start OrderForge.AppHost again; Keycloak will import from Keycloak/orderforge-realm.json." -ForegroundColor Green
