using AzureOpenAIProxy.Management.Components.ModelManagement;
using AzureOpenAIProxy.Management.Database;

namespace AzureOpenAIProxy.Management.Services;

public interface IModelService
{
    Task<OwnerCatalog> AddOwnerCatalogAsync(ModelEditorModel model);
    Task<IEnumerable<OwnerCatalog>> GetOwnerCatalogsAsync();
}
