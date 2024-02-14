using System.Data.Common;
using System.Security.Claims;
using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Npgsql;

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

        using DbConnection conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();

        cmd.CommandText = $"SELECT * FROM aoai.owner WHERE owner_id = @owner_id";
        cmd.Parameters.Add(new NpgsqlParameter("owner_id", id));
        var reader = await cmd.ExecuteReaderAsync();

        if (reader.HasRows)
        {
            await reader.DisposeAsync();
            await conn.CloseAsync();

            logger.LogInformation("User {id} already registered", id);
            return;
        }

        await reader.DisposeAsync();

        cmd = conn.CreateCommand();
        cmd.CommandText = $"INSERT INTO aoai.owner (owner_id, email, name) VALUES (@owner_id, @email, @name)";
        cmd.Parameters.Add(new NpgsqlParameter("owner_id", id));
        cmd.Parameters.Add(new NpgsqlParameter("email", principal.Identity?.Name ?? "Unknown"));
        cmd.Parameters.Add(new NpgsqlParameter("name", principal.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "Unknown"));
        await cmd.ExecuteNonQueryAsync();
        logger.LogInformation("User {id} registered", id);

        await reader.DisposeAsync();
        await conn.CloseAsync();
    }

    public static string GetEntraId(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? throw new ApplicationException("id is null");
    }
}
