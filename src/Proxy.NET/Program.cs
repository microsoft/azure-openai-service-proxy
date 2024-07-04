using System.Net;
using System.Text.Json;
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

builder.Services.AddDbContext<AoaiProxyContext>(
    (option) =>
    {
        option.UseNpgsql(
            dataSource,
            // https://stackoverflow.com/questions/70423137/how-to-gracefully-handle-a-postgres-restart-in-npgsql
            (options) =>
            {
                options.EnableRetryOnFailure(4, TimeSpan.FromSeconds(30), ["57P01"]);
            }
        );
    }
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

// calling the next middleware if successful.
// global catch exceptions and log them to App Insights
app.Use(
    async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        try
        {
            await next.Invoke();
        }
        catch (JsonException ex)
        {
            logger.LogInformation(ex, "JSON exception.");
            await WriteOpenAIExceptionFormat(context, ex.Message, (int)HttpStatusCode.BadRequest);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Argument exception.");
            await WriteOpenAIExceptionFormat(context, ex.Message, (int)HttpStatusCode.BadRequest);
        }
        catch (HttpRequestException ex)
        {
            logger.LogInformation(ex, "An error occurred while processing the request.");
            await WriteOpenAIExceptionFormat(context, ex.Message, (int?)ex.StatusCode ?? (int)HttpStatusCode.ServiceUnavailable);
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error.");
            await WriteOpenAIExceptionFormat(context, ex.Message, (int)HttpStatusCode.ServiceUnavailable);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "The request was canceled.");
            await WriteOpenAIExceptionFormat(context, ex.Message, (int)HttpStatusCode.GatewayTimeout);
        }
        catch (BadHttpRequestException ex)
        {
            logger.LogWarning(ex, "The request was bad.");
            await WriteOpenAIExceptionFormat(context, ex.Message, (int)HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred.");
            await WriteOpenAIExceptionFormat(context, ex.Message, (int)HttpStatusCode.InternalServerError);
        }
    }
);

app.Run();

/// <summary>
/// Writes the OpenAI exception format to the HTTP response.
/// </summary>
/// <param name="context">The HttpContext.</param>
/// <param name="msg">The error message.</param>
/// <param name="statusCode">The status code.</param>
/// <returns>A task representing the asynchronous operation.</returns>
static async Task WriteOpenAIExceptionFormat(HttpContext context, string msg, int statusCode)
{
    var error = new OpenAIError(statusCode, msg);
    await Results.Json(new { error }, statusCode: statusCode).ExecuteAsync(context);
}

record OpenAIError(int Code, string Message);
