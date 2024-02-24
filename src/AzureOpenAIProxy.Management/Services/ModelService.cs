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
            FriendlyName = model.FriendlyName,
            DeploymentName = model.DeploymentName!,
            EndpointKey = model.EndpointKey!,
            Location = model.Location!,
            ModelType = model.ModelType!.Value,
            EndpointUrl = model.EndpointUrl!
        };

        db.OwnerCatalogs.Add(catalog);
        db.SaveChanges();

        return catalog;
    }

    public async Task<IEnumerable<OwnerCatalog>> GetOwnerCatalogsAsync()
    {
        string entraId = await authService.GetCurrentUserEntraIdAsync();
        return db.OwnerCatalogs.Where(oc => oc.Owner.OwnerId == entraId).ToList();
    }

}
