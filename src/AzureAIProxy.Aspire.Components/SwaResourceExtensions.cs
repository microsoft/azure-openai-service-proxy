namespace AzureAIProxy.Aspire.Components;
public static class SwaResourceExtensions
{
    public static IResourceBuilder<SwaResource> AddSwaEmulator(this IDistributedApplicationBuilder builder, string name)
    {
        var resource = new SwaResource(name, Environment.CurrentDirectory);
        int port = 4280;
        return builder.AddResource(resource)
            .WithHttpEndpoint(isProxied: false, port: port)
            .WithArgs(ctx =>
            {
                ctx.Args.Add("start");

                if (resource.AppResource is not null && resource.AppEndpoint is not null)
                {
                    ctx.Args.Add("--app-devserver-url");
                    ctx.Args.Add(resource.AppEndpoint);
                }

                if (resource.ApiResource is not null && resource.ApiEndpoint is not null)
                {
                    ctx.Args.Add("--api-devserver-url");
                    ctx.Args.Add(resource.ApiEndpoint);
                }

                ctx.Args.Add("--port");
                ctx.Args.Add(port.ToString());
            });
    }

    public static IResourceBuilder<SwaResource> WithAppResource(this IResourceBuilder<SwaResource> builder, IResourceBuilder<IResourceWithEndpoints> appResource)
    {
        builder.Resource.AppResource = appResource;
        return builder;
    }

    public static IResourceBuilder<SwaResource> WithApiResource(this IResourceBuilder<SwaResource> builder, IResourceBuilder<IResourceWithEndpoints> apiResource)
    {
        builder.Resource.ApiResource = apiResource;
        return builder;
    }
}
