using AzureOpenAIProxy.Management;
using AzureOpenAIProxy.Management.Components;
using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddAuth();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler();

builder.Services.AddDbContext<AoaiProxyContext>(options =>
{
    NpgsqlDataSourceBuilder dataSourceBuilder = new(builder.Configuration.GetConnectionString("AoaiProxyContext"));
    dataSourceBuilder.MapEnum<ModelType>();
    NpgsqlDataSource dataSource = dataSourceBuilder.Build();
    options.UseNpgsql(dataSource);
});

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IModelService, ModelService>();

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
