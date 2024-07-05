using Azure.Core;
using Azure.Identity;
using AzureOpenAIProxy.Management;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Proxy.NET.Authentication;
using Proxy.NET.Routes;
using Proxy.NET.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

NpgsqlDataSourceBuilder dataSourceBuilder = new(builder.Configuration.GetConnectionString("AoaiProxyContext"));
dataSourceBuilder.MapEnum<ModelType>();

if (string.IsNullOrEmpty(dataSourceBuilder.ConnectionStringBuilder.Password))
{
    // If no password was provided, we're probably going to be using a managed identity to authenticate.
    // Define the time period the token lives for, and how long to wait to retry if the token fails.
    int refreshPeriod = builder.Configuration.GetValue("Azure:TokenRefreshSeconds", 14400);
    int failureRefreshPeriod = builder.Configuration.GetValue("Azure:TokenFailureRefreshSeconds", 10);

    // Use the Azure SDK to get a token from the managed identity and provide it to Npgsql using the
    // PeriodicPasswordProvider to refresh it on schedule.
    dataSourceBuilder.UsePeriodicPasswordProvider(
        async (_, ct) =>
        {
            var credential = new DefaultAzureCredential();
            var ctx = new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]);
            var tokenResponse = await credential.GetTokenAsync(ctx, ct);
            return tokenResponse.Token;
        },
        TimeSpan.FromSeconds(refreshPeriod),
        TimeSpan.FromSeconds(failureRefreshPeriod)
    );
}

NpgsqlDataSource dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AoaiProxyContext>((option) =>
option.UseNpgsql(
    dataSource,
    // https://stackoverflow.com/questions/70423137/how-to-gracefully-handle-a-postgres-restart-in-npgsql
    (options) => options.EnableRetryOnFailure(4, TimeSpan.FromSeconds(30), ["57P01"]))
);

builder.Services.AddAuthorization();
builder
    .Services.AddAuthentication()
    .AddScheme<ProxyAuthenticationOptions, ApiKeyAuthenticationHandler>(ProxyAuthenticationOptions.ApiKeyScheme, _ => { })
    .AddScheme<ProxyAuthenticationOptions, JwtAuthenticationHandler>(ProxyAuthenticationOptions.JwtScheme, _ => { });

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IProxyService, ProxyService>();
builder.Services.AddProxyServices();
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapProxyRoutes();

app.Run();
