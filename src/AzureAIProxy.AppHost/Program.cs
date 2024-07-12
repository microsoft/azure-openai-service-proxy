using AzureAIProxy.Aspire.Components;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("pg")
    .WithBindMount("../../database/aoai-proxy-dev.sql", "/docker-entrypoint-initdb.d/01-aoai-proxy.sql")
    .WithBindMount("../../database/aoai-proxy.sql", "/docker-entrypoint-initdb.d/02-aoai-proxy.sql")
    .WithDataVolume()
    .WithPgAdmin();

var db = postgres.AddDatabase("postgres");

var proxy = builder.AddProject<Projects.AzureAIProxy>("proxy")
    .WithReference(db)
    .WithExternalHttpEndpoints();

var playground = builder.AddNpmApp("playground", "../playground", scriptName: "dev")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

_ = builder.AddProject<Projects.AzureAIProxy_Management>("admin")
    .WithReference(playground)
    .WithReference(db)
    .WithExternalHttpEndpoints();

_ = builder.AddSwaEmulator("swa")
    .WithAppResource(playground)
    .WithApiResource(proxy);

builder.Build().Run();
