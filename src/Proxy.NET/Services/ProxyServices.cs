namespace Proxy.NET.Services;

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
            .AddScoped<IEventService, EventService>();

        if (useMockProxy)
            services.AddScoped<IProxyService, MockProxyService>();
        else
            services.AddScoped<IProxyService, ProxyService>();

        return services;
    }
}
