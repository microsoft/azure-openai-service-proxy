using System.Data;
using AzureOpenAIProxy.Management.Components.ModelManagement;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace AzureOpenAIProxy.Management.Services;

public class ModelService(IAuthService authService, AoaiProxyContext db, IConfiguration configuration) : IModelService
{

    private const string PostgresEncryptionKey = "PostgresEncryptionKey";
    private readonly NpgsqlConnection connection = (NpgsqlConnection)db.Database.GetDbConnection();

    private async Task<byte[]?> PostgresEncryptValue(string value)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        string? postgresEncryptionKey = configuration[PostgresEncryptionKey];

        using var command = new NpgsqlCommand($"SELECT aoai.pgp_sym_encrypt('{value}', '{postgresEncryptionKey}');", connection);
        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return reader[0] as byte[];
    }

    private async Task<string?> PostgresDecryptValue(byte[] value)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        string? postgresEncryptionKey = configuration[PostgresEncryptionKey];

        using var command = new NpgsqlCommand($"SELECT aoai.pgp_sym_decrypt(@value, '{postgresEncryptionKey}')", connection);
        command.Parameters.AddWithValue("value", NpgsqlDbType.Bytea, value);
        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return reader[0] as string;
    }

    public async Task<OwnerCatalog> AddOwnerCatalogAsync(ModelEditorModel model)
    {
        Owner owner = await authService.GetCurrentOwnerAsync();

        byte[]? endpointKey = await PostgresEncryptValue(model.EndpointKey!);
        byte[]? endpointUrl = await PostgresEncryptValue(model.EndpointUrl!);

        OwnerCatalog catalog = new()
        {
            Owner = owner,
            Active = model.Active,
            FriendlyName = model.FriendlyName!,
            DeploymentName = model.DeploymentName!,
            Location = model.Location!,
            ModelType = model.ModelType!.Value,
            EndpointKeyEncrypted = endpointKey!,
            EndpointUrlEncrypted = endpointUrl!
        };

        await db.OwnerCatalogs.AddAsync(catalog);
        await db.SaveChangesAsync();

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
        var result = await db.OwnerCatalogs.FindAsync(catalogId);

        if (result is null)
        {
            return null!;
        }

        string? endpointKey = await PostgresDecryptValue(result.EndpointKeyEncrypted);
        string? endpointUrl = await PostgresDecryptValue(result.EndpointUrlEncrypted);

        result.EndpointKey = endpointKey!;
        result.EndpointUrl = endpointUrl!;

        return result;
    }

    public async Task<IEnumerable<OwnerCatalog>> GetOwnerCatalogsAsync()
    {
        string entraId = await authService.GetCurrentUserEntraIdAsync();
        var catalogItems = await db.OwnerCatalogs.Where(oc => oc.Owner.OwnerId == entraId).OrderBy(oc => oc.FriendlyName).ToListAsync();

        foreach (var catalog in catalogItems)
        {
            string? endpointUrl = await PostgresDecryptValue(catalog.EndpointUrlEncrypted);
            catalog.EndpointUrl = endpointUrl!;
        }

        return catalogItems;
    }


    public async Task UpdateOwnerCatalogAsync(Guid catalogId, OwnerCatalog ownerCatalog)
    {
        OwnerCatalog? existingCatalog = await db.OwnerCatalogs.FindAsync(catalogId);

        if (existingCatalog is null)
        {
            return;
        }

        byte[]? endpointKey = await PostgresEncryptValue(ownerCatalog.EndpointKey);
        byte[]? endpointUrl = await PostgresEncryptValue(ownerCatalog.EndpointUrl);

        existingCatalog.FriendlyName = ownerCatalog.FriendlyName;
        existingCatalog.DeploymentName = ownerCatalog.DeploymentName;
        existingCatalog.ModelType = ownerCatalog.ModelType;
        existingCatalog.Location = ownerCatalog.Location;
        existingCatalog.Active = ownerCatalog.Active;
        existingCatalog.EndpointKeyEncrypted = endpointKey!;
        existingCatalog.EndpointUrlEncrypted = endpointUrl!;

        db.OwnerCatalogs.Update(existingCatalog);
        await db.SaveChangesAsync();
    }
}
