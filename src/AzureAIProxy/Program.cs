using AzureAIProxy.Middleware;
using AzureAIProxy.Routes;
using AzureAIProxy.Services;
using AzureAIProxy.Shared;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
var useMockProxy = builder.Configuration.GetValue<bool>("UseMockProxy", false);

builder.AddAzureAIProxyDbContext();

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
    )
    .AddScheme<ProxyAuthenticationOptions, BearerTokenAuthenticationHandler>(
        ProxyAuthenticationOptions.BearerTokenScheme,
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
