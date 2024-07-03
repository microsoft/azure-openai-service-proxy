namespace Proxy.NET.Services;

public static class ServicesExtensions
{
    public static IServiceCollection AddProxyServices(this IServiceCollection services) =>
        services
            .AddScoped<ICatalogService, CatalogService>()
            .AddScoped<IAuthorizeService, AuthorizeService>()
            .AddScoped<IProxyService, ProxyService>()
            .AddScoped<IMetricService, MetricService>()
            .AddScoped<IAttendeeService, AttendeeService>()
            .AddScoped<IEventService, EventService>()
            .AddScoped<IRequestService, RequestService>();
}
