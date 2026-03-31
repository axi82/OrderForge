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

// API - fixed port 8080
var api = builder
    .AddProject<Projects.OrderForge_Api>("api", launchProfileName: null)
    .WithReference(orderforgeDb)
    .WaitFor(orderforgeDb)
    .WaitFor(seq)
    .WithEnvironment("Seq__ServerUrl", seq.GetEndpoint("http"))
    .WithEnvironment("Authentication__Authority", "")
    .WithEnvironment("OrderForge__RunUnderAspire", "true")
    .WithHttpEndpoint(port: 8080, name: "http", isProxied: false);

// Web - fixed port 4200
var web = builder
    .AddProject<Projects.OrderForge_Client>("web", launchProfileName: null)
    .WithReference(api)
    .WithEnvironment("ApiBaseUrl", $"{api.GetEndpoint("http")}/")
    .WithHttpEndpoint(port: 4200, name: "http", isProxied: false);

builder.Build().Run();