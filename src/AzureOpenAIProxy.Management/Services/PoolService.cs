using Azure.Identity;
using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AzureOpenAIProxy.Management;

public class PoolService(IConfiguration configuration) : IPoolService
{
    private readonly string? _pgConnectionString = configuration.GetConnectionString("AoaiProxyContext");
    private readonly string? _dbHost = configuration["POSTGRES_SERVER"];
    private readonly string? _dbName = configuration["POSTGRES_DATABASE"] ?? "aoai-proxy";
    private readonly string? _dbUser = configuration["POSTGRES_USER"];
    private readonly string? _dbPassword = configuration["POSTGRES_PASSWORD"];
    private DbContextOptionsBuilder<AoaiProxyContext>? _pgOptionsBuilder = null;
    private NpgsqlDataSourceBuilder? _pgDataSourceBuilder = null;
    private NpgsqlDataSource? _pgDataSource = null;

    private const int SucessTokenRefreshMinutes = 60;
    private const int FailureTokenRefreshMinutes = 10;

    private string GetConnectionString()
    {
        if (!string.IsNullOrEmpty(_pgConnectionString))
        {
            Console.WriteLine("Using Postgres Connection String");
            return _pgConnectionString;
        }

        if (!string.IsNullOrEmpty(_dbPassword))
        {
            Console.WriteLine("Using Postgres Native Authorization");
            return $"Host={_dbHost};Database={_dbName};Username={_dbUser};Password={_dbPassword}";
        }

        // note, no password in the connection string as we will use the periodic password provider
        Console.WriteLine("Using Postgres Entra Authorization");
        return $"Host={_dbHost};Database={_dbName};Username={_dbUser}";
    }

    async private Task<string> GetConnectionPassword()
    {
        if (!string.IsNullOrEmpty(_dbPassword))
        {
            return _dbPassword;
        }

        var sqlServerTokenProvider = new DefaultAzureCredential();
        string accessToken = (await sqlServerTokenProvider.GetTokenAsync(
             new Azure.Core.TokenRequestContext(scopes: new string[] { "https://ossrdbms-aad.database.windows.net/.default" }) { })).Token;
        return accessToken;
    }

    private void CreatePool()
    {

        Func<NpgsqlConnectionStringBuilder, CancellationToken, ValueTask<string>>? passwordProvider = async (builder, cancellationToken) =>
        {
            builder.Password = await GetConnectionPassword();
            // clear out connections in the pool that used the old connection token
            NpgsqlConnection.ClearAllPools();
            return builder.Password; // Add this line to return the password
        };

        Console.WriteLine("Generating new Postgres Connection");
        string connectionString = GetConnectionString();

        _pgDataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        _pgDataSourceBuilder.MapEnum<ModelType>();

        // If we have a connection string, we don't need to use the periodic password provider
        if (string.IsNullOrEmpty(_pgConnectionString))
        {
            _pgDataSourceBuilder.UsePeriodicPasswordProvider(passwordProvider, TimeSpan.FromMinutes(SucessTokenRefreshMinutes), TimeSpan.FromMinutes(FailureTokenRefreshMinutes));
        }

        _pgDataSource = _pgDataSourceBuilder.Build();
        _pgOptionsBuilder = new DbContextOptionsBuilder<AoaiProxyContext>();
        _pgOptionsBuilder.UseNpgsql(_pgDataSource);
    }

    public AoaiProxyContext GetNewDbContext()
    {
        if (_pgOptionsBuilder is null)
        {
            CreatePool();
        }
        return new AoaiProxyContext(_pgOptionsBuilder!.Options);
    }
}
