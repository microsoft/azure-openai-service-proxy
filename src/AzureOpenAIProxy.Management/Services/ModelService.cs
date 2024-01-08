using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Services;

public class ModelService(IAuthService authService, AoaiProxyContext db) : IModelService
{
    public async Task<IEnumerable<OwnerCatalog>> GetOwnerCatalogsAsync()
    {
        string entraId = await authService.GetCurrentUserEntraIdAsync();
        return await db.OwnerCatalogs.Where(oc => oc.Owner.EntraId == entraId).ToListAsync();
    }

}
