using AzureAIProxy.Aspire.Components;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var db = postgres.AddDatabase("AoaiProxyContext");

var proxy = builder.AddProject<Projects.AzureAIProxy>("proxy")
    .WithReference(db);

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
