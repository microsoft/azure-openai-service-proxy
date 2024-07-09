using AzureAIProxy.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using AzureAIProxy.Middleware;
using AzureAIProxy.Routes;
using AzureAIProxy.Services;
using AzureAIProxy.Shared;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
var useMockProxy = builder.Configuration.GetValue<bool>("UseMockProxy", false);

NpgsqlDataSourceBuilder dataSourceBuilder =
    new(builder.Configuration.GetConnectionString("AoaiProxyContext"));
dataSourceBuilder.MapEnum<ModelType>();

dataSourceBuilder.UseEntraAuth(builder.Configuration);

NpgsqlDataSource dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AzureAIProxyContext>(
    (option) =>
        option.UseNpgsql(
            dataSource,
            // https://stackoverflow.com/questions/70423137/how-to-gracefully-handle-a-postgres-restart-in-npgsql
            (options) => options.EnableRetryOnFailure(4, TimeSpan.FromSeconds(30), ["57P01"])
        )
);

builder.Services.AddAuthorization();
builder
    .Services.AddAuthentication()
    .AddScheme<ProxyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ProxyAuthenticationOptions.ApiKeyScheme,
        _ => { }
    )
    .AddScheme<ProxyAuthenticationOptions, JwtAuthenticationHandler>(
        ProxyAuthenticationOptions.JwtScheme,
        _ => { }
    );

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IProxyService, ProxyService>();
builder.Services.AddProxyServices(useMockProxy);
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RateLimiterHandler>();
app.MapProxyRoutes();

app.Run();
