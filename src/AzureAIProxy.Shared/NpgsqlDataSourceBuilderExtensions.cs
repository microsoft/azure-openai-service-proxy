using Azure.Core;
using Azure.Identity;
using AzureAIProxy.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace AzureAIProxy.Shared;

public static class NpgsqlDataSourceBuilderExtensions
{
    public static IHostApplicationBuilder AddAzureAIProxyDbContext(this IHostApplicationBuilder builder)
    {
        NpgsqlDataSourceBuilder dataSourceBuilder =
            new(builder.Configuration.GetConnectionString("AoaiProxyContext"));
        dataSourceBuilder.MapEnum<ModelType>();
        dataSourceBuilder.MapEnum<AssistantType>();

        dataSourceBuilder.UseEntraAuth(builder.Configuration);

        NpgsqlDataSource dataSource = dataSourceBuilder.Build();

        builder.Services.AddDbContext<AzureAIProxyDbContext>(
            (option) =>
                option.UseNpgsql(
                    dataSource,
                    // https://stackoverflow.com/questions/70423137/how-to-gracefully-handle-a-postgres-restart-in-npgsql
                    (options) => options.EnableRetryOnFailure(4, TimeSpan.FromSeconds(30), ["57P01"])
                )
        );

        return builder;
    }

    public static NpgsqlDataSourceBuilder UseEntraAuth(this NpgsqlDataSourceBuilder dataSourceBuilder, IConfiguration configuration)
    {
        if (string.IsNullOrEmpty(dataSourceBuilder.ConnectionStringBuilder.Password))
        {
            // If no password was provided, we're probably going to be using a managed identity to authenticate.
            // Define the time period the token lives for, and how long to wait to retry if the token fails.
            int refreshPeriod = configuration.GetValue("Azure:TokenRefreshSeconds", 14400);
            int failureRefreshPeriod = configuration.GetValue(
                "Azure:TokenFailureRefreshSeconds",
                10
            );

            // Use the Azure SDK to get a token from the managed identity and provide it to Npgsql using the
            // PeriodicPasswordProvider to refresh it on schedule.
            dataSourceBuilder.UsePeriodicPasswordProvider(
                async (_, ct) =>
                {
                    var credential = new DefaultAzureCredential();
                    var ctx = new TokenRequestContext(
                        ["https://ossrdbms-aad.database.windows.net/.default"]
                    );
                    var tokenResponse = await credential.GetTokenAsync(ctx, ct);
                    return tokenResponse.Token;
                },
                TimeSpan.FromSeconds(refreshPeriod),
                TimeSpan.FromSeconds(failureRefreshPeriod)
            );
        }
        return dataSourceBuilder;
    }
}
