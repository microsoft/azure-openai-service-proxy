using AzureOpenAIProxy.Management;
using AzureOpenAIProxy.Management.Components;
using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using Npgsql;
using Azure.Identity;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connection_string = builder.Configuration.GetConnectionString("AoaiProxyContext");
if (string.IsNullOrEmpty(connection_string))
{
    int db_port = 5432;
    string? db_host = builder.Configuration["POSTGRES_SERVER"];
    string? db_name = builder.Configuration["POSTGRES_DATABASE"];
    if (string.IsNullOrEmpty(db_name))
    {
        db_name = "aoai-proxy";
    }
    string? db_user = builder.Configuration["POSTGRES_USER"];
    string? db_password = builder.Configuration["POSTGRES_PASSWORD"];

    if (string.IsNullOrEmpty(db_host) || string.IsNullOrEmpty(db_user))
    {
        throw new Exception("Database connection string not found and POSTGRES_SERVER, POSTGRES_USER not set");
    }
    else
    {
        if (string.IsNullOrEmpty(db_password))
        {
            var sqlServerTokenProvider = new DefaultAzureCredential();
            string accessToken = (await sqlServerTokenProvider.GetTokenAsync(
                 new Azure.Core.TokenRequestContext(scopes: new string[] { "https://ossrdbms-aad.database.windows.net/.default" }) { })).Token;
            connection_string = $"Host={db_host};Port={db_port};Database={db_name};Username={db_user};Password={accessToken}";
            Console.WriteLine("Using Postgres Entra Authorization");
        }
        else
        {
            connection_string = $"Host={db_host};Port={db_port};Database={db_name};Username={db_user};Password={db_password}";
        }
    }
}

NpgsqlDataSourceBuilder dataSourceBuilder = new(connection_string);
dataSourceBuilder.MapEnum<ModelType>();
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
