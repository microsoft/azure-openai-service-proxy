using AzureAIProxy.Middleware;
using AzureAIProxy.Routes;
using AzureAIProxy.Services;
using AzureAIProxy.Shared;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

var useMockProxy = builder.Configuration.GetValue("UseMockProxy", false);
builder.Services.AddProxyServices(useMockProxy);

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RateLimiterHandler>();
app.UseMiddleware<LoadProperties>();
app.UseMiddleware<MaxTokensHandler>();
app.MapProxyRoutes();

app.Run();
