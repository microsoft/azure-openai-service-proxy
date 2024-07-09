using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AzureAIProxy.Shared;

public static class NpgsqlDataSourceBuilderExtensions
{
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
