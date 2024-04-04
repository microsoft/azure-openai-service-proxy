

using AzureOpenAIProxy.Management.Components.ModelManagement;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using System.Data.Common;
using NpgsqlTypes;


namespace AzureOpenAIProxy.Management.Services;

public class ModelService(IAuthService authService, AoaiProxyContext db, IConfiguration configuration) : IModelService
{
    private const string PostgressEncryptionKey = "PostgressEncryptionKey";

    private readonly NpgsqlConnection connection = (NpgsqlConnection)db.Database.GetDbConnection();

    public async Task<OwnerCatalog> AddOwnerCatalogAsync(ModelEditorModel model)
    {

        Owner owner = await authService.GetCurrentOwnerAsync();

        var postgressEncryptionKey = configuration.GetValue<string>(PostgressEncryptionKey);

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();

        command.CommandText = "SELECT aoai.add_owner_catalog(@p_owner_id, @p_deployment_name, @p_endpoint_url, @p_endpoint_key, @p_active, @p_model_type, @p_location, @p_friendly_name, @p_postgres_encryption_key)";

        command.Parameters.Add(new NpgsqlParameter("p_owner_id", NpgsqlDbType.Text) { Value = owner.OwnerId! });
        command.Parameters.Add(new NpgsqlParameter("p_deployment_name", NpgsqlDbType.Text) { Value = model.DeploymentName! });
        command.Parameters.Add(new NpgsqlParameter("p_endpoint_url", NpgsqlDbType.Text) { Value = model.EndpointUrl! });
        command.Parameters.Add(new NpgsqlParameter("p_endpoint_key", NpgsqlDbType.Text) { Value = model.EndpointKey! });
        command.Parameters.Add(new NpgsqlParameter("p_active", NpgsqlDbType.Boolean) { Value = model.Active });
        command.Parameters.Add(new NpgsqlParameter("p_model_type", NpgsqlDbType.Text) { Value = model.ModelType!.Value.ToPostgresValue() });
        command.Parameters.Add(new NpgsqlParameter("p_location", NpgsqlDbType.Text) { Value = model.Location! });
        command.Parameters.Add(new NpgsqlParameter("p_friendly_name", NpgsqlDbType.Text) { Value = model.FriendlyName! });
        command.Parameters.Add(new NpgsqlParameter("p_postgres_encryption_key", NpgsqlDbType.Text) { Value = postgressEncryptionKey });

        await command.ExecuteNonQueryAsync();

        OwnerCatalog catalog = new()
        {
            Owner = owner,
            Active = model.Active,
            FriendlyName = model.FriendlyName!,
            DeploymentName = model.DeploymentName!,
            // EndpointKey = model.EndpointKey!,
            // EndpointUrl = model.EndpointUrl!,
            Location = model.Location!,
            ModelType = model.ModelType!.Value,
        };

        // await db.OwnerCatalogs.AddAsync(catalog);
        // await db.SaveChangesAsync();

        return catalog;
    }

    public async Task DeleteOwnerCatalogAsync(Guid catalogId)
    {
        OwnerCatalog? ownerCatalog = await db.OwnerCatalogs.FindAsync(catalogId);

        if (ownerCatalog is null)
        {
            return;
        }

        // find if the resource is used in an event or has metrics
        var usageInfo = await db.OwnerCatalogs.Where(oc => oc.CatalogId == catalogId)
            .Select(oc => new
            {
                UsedInEvent = oc.Events.Count != 0
            })
            .FirstAsync();

        // block deletion when it's in use to avoid cascading deletes
        if (usageInfo.UsedInEvent)
        {
            return;
        }

        db.OwnerCatalogs.Remove(ownerCatalog);
        await db.SaveChangesAsync();
    }

    public async Task<OwnerCatalog> GetOwnerCatalogAsync(Guid catalogId)
    {
        var postgressEncryptionKey = configuration.GetValue<string>(PostgressEncryptionKey);

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM aoai.get_owner_catalog(@p_catalog_id, @p_postgres_encryption_key)";

        command.Parameters.Add(new NpgsqlParameter("p_catalog_id", NpgsqlDbType.Uuid) { Value = catalogId });
        command.Parameters.Add(new NpgsqlParameter("p_postgres_encryption_key", NpgsqlDbType.Text) { Value = postgressEncryptionKey });

        using NpgsqlDataReader reader = (NpgsqlDataReader)await command.ExecuteReaderAsync();

        OwnerCatalog ownerCatalog = new();

        if (reader.HasRows)
        {
            await reader.ReadAsync();

            ownerCatalog = new()
            {
                OwnerId = reader.GetString(0),
                CatalogId = reader.GetGuid(1),
                DeploymentName = reader.GetString(2),
                // EndpointUrl = reader.GetString(3),
                // EndpointKey = reader.GetString(4),
                Active = reader.GetBoolean(5),
                ModelType = ModelTypeExtensions.ParsePostgresValue(reader.GetString(6)),
                Location = reader.GetString(7),
                FriendlyName = reader.GetString(8),

            };
        }
        return ownerCatalog;
    }

    public async Task<IEnumerable<OwnerCatalog>> GetOwnerCatalogsAsync()
    {
        string entraId = await authService.GetCurrentUserEntraIdAsync();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();

        command.CommandText = "SELECT owner_id, catalog_id, deployment_name, active, model_type, location, friendly_name FROM aoai.owner_catalog WHERE owner_id = @owner_id ORDER BY friendly_name;";
        command.Parameters.Add(new NpgsqlParameter("owner_id", NpgsqlDbType.Text) { Value = entraId });

        using NpgsqlDataReader reader = (NpgsqlDataReader)await command.ExecuteReaderAsync();

        List<OwnerCatalog> ownerCatalogs = new();

        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
            {
                OwnerCatalog ownerCatalog = new()
                {
                    OwnerId = reader.GetString(0),
                    CatalogId = reader.GetGuid(1),
                    DeploymentName = reader.GetString(2),
                    Active = reader.GetBoolean(3),
                    ModelType = ModelTypeExtensions.ParsePostgresValue(reader.GetString(4)),
                    Location = reader.GetString(5),
                    FriendlyName = reader.GetString(6)
                };

                ownerCatalogs.Add(ownerCatalog);
            }
        }

        return ownerCatalogs;

        // return await db.OwnerCatalogs.Where(oc => oc.Owner.OwnerId == entraId).OrderBy(oc => oc.FriendlyName).ToListAsync();
    }

    public async Task UpdateOwnerCatalogAsync(Guid catalogId, OwnerCatalog ownerCatalog)
    {

        var postgressEncryptionKey = configuration.GetValue<string>(PostgressEncryptionKey);

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();

        command.CommandText = "SELECT aoai.update_owner_catalog(@p_catalog_id, @p_deployment_name, @p_endpoint_url, @p_endpoint_key, @p_active, @p_model_type, @p_location, @p_friendly_name, @p_postgres_encryption_key)";

        command.Parameters.Add(new NpgsqlParameter("p_catalog_id", NpgsqlDbType.Uuid) { Value = catalogId });
        command.Parameters.Add(new NpgsqlParameter("p_deployment_name", NpgsqlDbType.Text) { Value = ownerCatalog.DeploymentName! });
        command.Parameters.Add(new NpgsqlParameter("p_endpoint_url", NpgsqlDbType.Text) { Value = ownerCatalog.EndpointUrl! });
        command.Parameters.Add(new NpgsqlParameter("p_endpoint_key", NpgsqlDbType.Text) { Value = ownerCatalog.EndpointKey! });
        command.Parameters.Add(new NpgsqlParameter("p_active", NpgsqlDbType.Boolean) { Value = ownerCatalog.Active });
        command.Parameters.Add(new NpgsqlParameter("p_model_type", NpgsqlDbType.Text) { Value = ownerCatalog.ModelType!.Value.ToPostgresValue() });
        command.Parameters.Add(new NpgsqlParameter("p_location", NpgsqlDbType.Text) { Value = ownerCatalog.Location! });
        command.Parameters.Add(new NpgsqlParameter("p_friendly_name", NpgsqlDbType.Text) { Value = ownerCatalog.FriendlyName! });
        command.Parameters.Add(new NpgsqlParameter("p_postgres_encryption_key", NpgsqlDbType.Text) { Value = postgressEncryptionKey });

        await command.ExecuteNonQueryAsync();
    }
}
