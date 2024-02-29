using AzureOpenAIProxy.Management.Components.ModelManagement;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Services;

public class ModelService(IAuthService authService, AoaiProxyContext db) : IModelService
{
    public async Task<OwnerCatalog> AddOwnerCatalogAsync(ModelEditorModel model)
    {
        Owner owner = await authService.GetCurrentOwnerAsync();

        OwnerCatalog catalog = new()
        {
            Owner = owner,
            Active = model.Active,
            FriendlyName = model.FriendlyName!,
            DeploymentName = model.DeploymentName!,
            EndpointKey = model.EndpointKey!,
            Location = model.Location!,
            ModelType = model.ModelType!.Value,
            EndpointUrl = model.EndpointUrl!
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

    public async Task<IEnumerable<OwnerCatalog>> GetOwnerCatalogsAsync()
    {
        string entraId = await authService.GetCurrentUserEntraIdAsync();
        return await db.OwnerCatalogs.Where(oc => oc.Owner.OwnerId == entraId).OrderBy(oc => oc.FriendlyName).ToListAsync();
    }

}
