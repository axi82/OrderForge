using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Postgres - using parameters (recommended way)
var postgresUser = builder.AddParameter("postgres-user", "orderforge", secret: false);
var postgresPassword = builder.AddParameter("postgres-password", "orderforge", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUser, postgresPassword, port: 5432)
    .WithDataVolume("orderforge-postgres-data");

var orderforgeDb = postgres.AddDatabase("orderforge");

// Seq: persist /data so the admin user and password survive container recreation.
// Default login is username "admin" and this password (override via user secrets / Aspire parameters UI).
var seqAdminPassword = builder.AddParameter("seq-admin-password", "dev-seq-admin-change-me", secret: true);
var seq = builder
    .AddSeq("seq", seqAdminPassword)
    .WithDataVolume("orderforge-seq-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .ExcludeFromManifest();

// Keycloak 26+ bootstraps a temporary master-realm admin; first login shows "Update Password" by design.
// Persist /opt/keycloak/data so after you submit that form once, restarts reuse the same admin (no repeat prompt).
// Credentials match docker-compose (KC_BOOTSTRAP_*); KEYCLOAK_* is still set by AddKeycloakContainer from parameters.
var keycloakAdmin = builder.AddParameter("keycloak-admin", "admin", secret: false);
var keycloakAdminPassword = builder.AddParameter("keycloak-admin-password", "admin", secret: true);
var keycloak = builder.AddKeycloakContainer(
        "keycloak",
        userName: keycloakAdmin,
        password: keycloakAdminPassword,
        port: 8081)
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", keycloakAdmin)
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithDataVolume("orderforge-keycloak-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .ExcludeFromManifest();

// Realm: first arg is the Aspire resource name (must be unique; postgres DB is already "orderforge").
// Second arg is the Keycloak realm name used in URLs: .../realms/orderforge
const string keycloakRealmName = "orderforge";
var orderforgeRealm = keycloak.AddRealm("keycloak-orderforge-realm", keycloakRealmName);

// API: AddProject already creates an "http" endpoint from Kestrel bindings — use a different name for the fixed host port.
var api = builder
    .AddProject<Projects.OrderForge_Api>("api", launchProfileName: null)
    .WithReference(orderforgeDb)
    .WithReference(orderforgeRealm)
    .WaitFor(orderforgeDb)
    .WaitFor(seq)
    .WaitFor(keycloak)
    .WithEnvironment("Seq__ServerUrl", seq.GetEndpoint("http"))
    //.WithEnvironment("Authentication__Authority", $"{keycloak.GetEndpoint("http")}/realms/{keycloakRealmName}")
    .WithEnvironment("Authentication__Audience", "orderforge-api")
    .WithEnvironment("OrderForge__RunUnderAspire", "true")
    .WithHttpEndpoint(port: 8080, name: "api-public", isProxied: false);

// Web: same rule — do not name a second endpoint "http" on this project resource.
var web = builder
    .AddProject<Projects.OrderForge_Client>("web", launchProfileName: null)
    .WithReference(api)
    .WaitFor(api)    
    .WithEnvironment("ApiBaseUrl", $"{api.GetEndpoint("api-public")}/")
    .WithHttpEndpoint(port: 4200, name: "web-public", isProxied: false);


builder.Build().Run();
