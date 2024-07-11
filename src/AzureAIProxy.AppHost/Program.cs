var builder = DistributedApplication.CreateBuilder(args);

var proxy = builder.AddProject<Projects.AzureAIProxy>("proxy");

var playground = builder.AddNpmApp("playground", "../playground", scriptName: "dev")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

var admin = builder.AddProject<Projects.AzureAIProxy_Management>("admin")
    .WithReference(playground)
    .WithExternalHttpEndpoints();

var swa = builder.AddExecutable("swa", "swa", Environment.CurrentDirectory)
    .WithArgs(ctx =>
    {
        ctx.Args.Add("start");
        ctx.Args.Add("--app-devserver-url");
        ctx.Args.Add(playground.GetEndpoint("http").Url);
        ctx.Args.Add("--api-devserver-url");
        ctx.Args.Add(proxy.GetEndpoint("http").Url);
    });

builder.Build().Run();
