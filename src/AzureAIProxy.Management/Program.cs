using Azure.Core;
using Azure.Identity;
using AzureAIProxy.Management;
using AzureAIProxy.Management.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using Npgsql;

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
    dataSourceBuilder.UsePeriodicPasswordProvider(async (_, ct) =>
    {
        var credential = new DefaultAzureCredential();
        var ctx = new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]);
        var tokenResponse = await credential.GetTokenAsync(ctx, ct);
        return tokenResponse.Token;
    }, TimeSpan.FromSeconds(refreshPeriod), TimeSpan.FromSeconds(failureRefreshPeriod));
}

NpgsqlDataSource dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AoaiProxyContext>((options) =>
{
    options.UseNpgsql(dataSource);
});

builder.AddAuth();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<IMetricService, MetricService>();

builder.Services.AddMudServices();

builder.Services.AddApplicationInsightsTelemetry();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.UseStaticFiles();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
