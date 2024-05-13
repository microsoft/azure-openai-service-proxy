using Azure.Identity;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AzureOpenAIProxy.Management
{
    public class CustomDbContextFactory(IConfiguration configuration) : IDbContextFactory<AoaiProxyContext>
    {
        private readonly string? _pgConnectionString = configuration.GetConnectionString("AoaiProxyContext");
        private readonly string? _dbHost = configuration["POSTGRES_SERVER"];
        private readonly string? _dbName = configuration["POSTGRES_DATABASE"];
        private readonly string? _dbUser = configuration["POSTGRES_USER"];
        private readonly string? _dbPassword = configuration["POSTGRES_PASSWORD"];
        private DbContextOptionsBuilder<AoaiProxyContext>? _pgOptionsBuilder = null;
        private NpgsqlDataSourceBuilder? _pgDataSourceBuilder = null;
        private NpgsqlDataSource? _pgDataSource = null;
        private DateTime _connectionTime = DateTime.MinValue;
        private const int _maxConnectionTime = 60 * 1; // 1 hrs

        async private Task<string> GetConnectionString()
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
            else
            {
                Console.WriteLine("Using Postgres Entra Authorization");
                var sqlServerTokenProvider = new DefaultAzureCredential();
                string accessToken = (await sqlServerTokenProvider.GetTokenAsync(
                     new Azure.Core.TokenRequestContext(scopes: new string[] { "https://ossrdbms-aad.database.windows.net/.default" }) { })).Token;
                return $"Host={_dbHost};Database={_dbName};Username={_dbUser};Password={accessToken}";
            }
        }

        AoaiProxyContext IDbContextFactory<AoaiProxyContext>.CreateDbContext()
        {
            if (_pgOptionsBuilder is not null && (DateTime.Now - _connectionTime).TotalMinutes < _maxConnectionTime)
            {
                return new AoaiProxyContext(_pgOptionsBuilder.Options);
            }

            // has the connection been created before?
            if (_pgOptionsBuilder is not null)
            {
                NpgsqlConnection.ClearAllPools();
            }

            Console.WriteLine("Generating new Postgres Connection");

            _connectionTime = DateTime.Now;
            string connectionString = GetConnectionString().GetAwaiter().GetResult();

            _pgDataSourceBuilder = new(connectionString);
            _pgDataSourceBuilder.MapEnum<ModelType>();
            _pgDataSource = _pgDataSourceBuilder.Build();
            _pgOptionsBuilder = new DbContextOptionsBuilder<AoaiProxyContext>();
            _pgOptionsBuilder.UseNpgsql(_pgDataSource);

            return new AoaiProxyContext(_pgOptionsBuilder.Options);
        }
    }
}
