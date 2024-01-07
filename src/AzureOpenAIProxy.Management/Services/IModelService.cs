using AzureOpenAIProxy.Management.Database;

namespace AzureOpenAIProxy.Management.Services;

public interface IModelService
{
    Task<IEnumerable<OwnerCatalog>> GetOwnerCatalogsAsync();
}
