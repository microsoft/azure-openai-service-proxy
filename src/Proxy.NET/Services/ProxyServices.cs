namespace Proxy.NET.Services;

public static class ServicesExtensions
{
    public static IServiceCollection AddProxyServices(this IServiceCollection services)
    {
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IAuthorizeService, AuthorizeService>();
        services.AddScoped<IProxyService, ProxyService>();
        services.AddScoped<IMetricService, MetricService>();
        services.AddScoped<IAttendeeService, AttendeeService>();
        services.AddScoped<IEventService, EventService>();

        return services;
    }
}
