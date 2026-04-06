Order Forge Keycloak theme: order-forge (ForgeClean)
====================================================

1. Mount this folder to /opt/keycloak/themes (see docker-compose.yml and AppHost Program.cs).

2. Realm "orderforge" must use Login theme: order-forge.
   - New imports: set in orderforge-realm.json (loginTheme).
   - Existing Keycloak DB: Admin Console -> Realm settings -> Themes -> Login theme -> order-forge -> Save.

3. Restart Keycloak after changing theme files, or use KC_SPI_THEME_CACHE_THEMES=false (already set for local dev).

Theme folder layout: themes/order-forge/login/theme.properties
