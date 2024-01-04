using AzureOpenAIProxy.Management;
using AzureOpenAIProxy.Management.Components;
using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler();

builder.Services.AddDbContext<AoaiProxyContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("AoaiProxyContext"));
});

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = async ctx =>
        {
            if (ctx.Principal is null)
                throw new ApplicationException("Principal is null");

            AoaiProxyContext db = ctx.HttpContext.RequestServices.GetRequiredService<AoaiProxyContext>();
            ILogger<Program> logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            string id = (ctx.Principal.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value) ?? throw new ApplicationException("id is null");

            if (await db.Owners.AnyAsync(o => o.EntraId == id))
            {
                logger.LogInformation("User {id} already registered", id);
                return;
            }

            Owner owner = new()
            {
                EntraId = id,
                OwnerId = Guid.NewGuid()
            };

            db.Owners.Add(owner);
            // Regsiter the current user
            await db.SaveChangesAsync();

            logger.LogInformation("User {id} registered", id);
        }
    };
});

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
