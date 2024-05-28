using System.Security.Claims;
using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace AzureOpenAIProxy.Management;

public static class AuthExtensions
{
    public static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {

        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
        // builder.Services.AddControllersWithViews()
        //     .AddMicrosoftIdentityUI();

        builder.Services.AddAuthorization(options =>
        {
            // By default, all incoming requests will be authorized according to the default policy
            options.FallbackPolicy = options.DefaultPolicy;
        });

        builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Events = new OpenIdConnectEvents
            {
                OnTokenValidated = TokenValidated
            };
        });

        // builder.Services.AddControllersWithViews()
        //     .AddMicrosoftIdentityUI();

        builder.Services.AddRazorPages()
            .AddMicrosoftIdentityUI();

        return builder;
    }

    private static async Task TokenValidated(TokenValidatedContext ctx)
    {
        ClaimsPrincipal? principal = ctx.Principal;
        if (principal is null)
            throw new ApplicationException("Principal is null");

        AoaiProxyContext db = ctx.HttpContext.RequestServices.GetRequiredService<AoaiProxyContext>();
        ILogger<Program> logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

        string id = principal.GetEntraId();

        if (await db.Owners.AnyAsync(o => o.OwnerId == id))
        {
            logger.LogInformation("User {id} already registered", id);
            return;
        }

        Owner owner = new()
        {
            OwnerId = id,
            Email = principal.Identity?.Name ?? "Unknown",
            Name = principal.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "Unknown"
        };

        db.Owners.Add(owner);
        // Regsiter the current user
        await db.SaveChangesAsync();

        logger.LogInformation("User {id} registered", id);
    }

    public static string GetEntraId(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? throw new ApplicationException("id is null");
    }
}
