Order Forge Keycloak theme: order-forge (ForgeClean)
====================================================

1. Mount this folder to /opt/keycloak/themes (see docker-compose.yml and AppHost Program.cs).

2. Realm "orderforge" must use Login theme: order-forge.
   - New imports: set in orderforge-realm.json (loginTheme).
   - Existing Keycloak DB: Admin Console -> Realm settings -> Themes -> Login theme -> order-forge -> Save.

3. Email theme (password reset, etc.): orderforge-email.
   - New imports: set in orderforge-realm.json (emailTheme).
   - Existing Keycloak DB: realm JSON is NOT re-applied; you must set the theme once on the live realm:
     Admin Console -> Realm settings -> Themes -> Email theme -> orderforge-email -> Save.
   - Or from repo root: pwsh ./scripts/set-keycloak-email-theme.ps1
     (uses KEYCLOAK_ADMIN / KEYCLOAK_ADMIN_PASSWORD from your environment or .env).

   Troubleshooting — you still see the default email ("Link to reset credentials", subject "Reset password"):
   The realm in Keycloak's database does not have emailTheme set. Fix with Admin Console or the script above,
   then trigger another password reset. Re-importing orderforge-realm.json only runs on first Keycloak DB
   creation; otherwise use scripts/reset-keycloak-dev.ps1 to wipe the Keycloak volume and re-import (destructive).

4. Restart Keycloak after changing theme files, or use KC_SPI_THEME_CACHE_THEMES=false (already set for local dev).

Theme folder layout:
  themes/order-forge/login/theme.properties
  themes/orderforge-email/email/theme.properties
