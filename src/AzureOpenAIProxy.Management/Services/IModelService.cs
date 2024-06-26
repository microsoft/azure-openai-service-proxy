using AzureOpenAIProxy.Management.Components.ModelManagement;

namespace AzureOpenAIProxy.Management.Services;

public interface IModelService
{
    Task<OwnerCatalog> AddOwnerCatalogAsync(ModelEditorModel model);
    Task DeleteOwnerCatalogAsync(Guid catalogId);
    Task<IEnumerable<OwnerCatalog>> GetOwnerCatalogsAsync();
    Task<OwnerCatalog> GetOwnerCatalogAsync(Guid catalogId);
    Task UpdateOwnerCatalogAsync(OwnerCatalog ownerCatalog);
    Task DuplicateOwnerCatalogAsync(OwnerCatalog ownerCatalog);
}
