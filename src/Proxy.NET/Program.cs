using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using AzureOpenAIProxy.Management;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Proxy.NET.Endpoints;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET;

internal class Program
{
    public record OpenAIError(int Code, string Message);

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddMemoryCache();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        builder.Services.AddHttpClient<IProxyService, ProxyService>();

        // builder.Services.AddScoped<ICatalogService, CatalogService>();
        // builder.Services.AddScoped<IAuthorizeService, AuthorizeService>();
        // builder.Services.AddScoped<IProxyService, ProxyService>();
        // builder.Services.AddScoped<IMetricService, MetricService>();
        // builder.Services.AddScoped<IAttendeeService, AttendeeService>();
        // builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddProxyServices();

        builder.Services.AddApplicationInsightsTelemetry();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapProxyRoutes();

        // validate the request and authorize the user.
        // calling the next middleware if successful.
        // global catch exceptions and log them to App Insights
        app.Use(
            async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                var authorizeService = context.RequestServices.GetRequiredService<IAuthorizeService>();
                try
                {
                    var endpoint = context.GetEndpoint();
                    var authType = endpoint?.Metadata.GetMetadata<Auth>()?.AuthType;

                    context.Items["RequestContext"] = authType switch
                    {
                        Auth.Type.ApiKey => await authorizeService.GetRequestContextByApiKey(context.Request.Headers),
                        Auth.Type.Jwt => authorizeService.GetRequestContextFromJwt(context.Request.Headers),
                        Auth.Type.None => null,
                        _ => throw new ArgumentException("Mismatched auth type or HTTP verb")
                    };

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
    }

    /// <summary>
    /// Writes the OpenAI exception format to the HTTP response.
    /// </summary>
    /// <param name="context">The HttpContext.</param>
    /// <param name="msg">The error message.</param>
    /// <param name="statusCode">The status code.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task WriteOpenAIExceptionFormat(HttpContext context, string msg, int statusCode)
    {
        var error = new OpenAIError(statusCode, msg);
        await Results.Json(new { error }, statusCode: statusCode).ExecuteAsync(context);
    }
}
