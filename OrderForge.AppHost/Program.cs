using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Postgres - using parameters (recommended way)
var postgresUser = builder.AddParameter("postgres-user", "orderforge", secret: false);
var postgresPassword = builder.AddParameter("postgres-password", "orderforge", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUser, postgresPassword, port: 5432)
    .WithDataVolume("orderforge-postgres-data");

var orderforgeDb = postgres.AddDatabase("orderforge");

// Seq
var seq = builder
    .AddSeq("seq")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SEQ_FIRSTRUN_ADMINPASSWORD", "dev-seq-admin-change-me")
    .WithLifetime(ContainerLifetime.Persistent)
    .ExcludeFromManifest();

// Keycloak defaults to host port 8080; the API also uses 8080 below — that causes "address already in use".
// Map Keycloak to 8081 on the host (container still listens on 8080 inside the image).
var keycloak = builder.AddKeycloakContainer("keycloak", port: 8081)
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
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
    .WithEnvironment("ApiBaseUrl", $"{api.GetEndpoint("api-public")}/")
    .WithHttpEndpoint(port: 4200, name: "web-public", isProxied: false);

builder.Build().Run();
