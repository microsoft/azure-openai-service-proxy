namespace AzureAIProxy.Services;

public static class ServicesExtensions
{
    public static IServiceCollection AddProxyServices(
        this IServiceCollection services,
        bool useMockProxy
    )
    {
        services
            .AddScoped<ICatalogService, CatalogService>()
            .AddScoped<IAuthorizeService, AuthorizeService>()
            .AddScoped<IMetricService, MetricService>()
            .AddScoped<IAttendeeService, AttendeeService>()
            .AddScoped<IEventService, EventService>()
            .AddScoped<IAssistantService, AssistantService>();

        if (useMockProxy)
            services.AddScoped<IProxyService, MockProxyService>();
        else
            services.AddScoped<IProxyService, ProxyService>();

        return services;
    }
}
