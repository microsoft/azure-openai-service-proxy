using AzureOpenAIProxy.Management.Services;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management;

public class DbContextService(IPoolService poolService) : IDbContextFactory<AoaiProxyContext>
{
    private AoaiProxyContext _dbContext = null!;

    AoaiProxyContext IDbContextFactory<AoaiProxyContext>.CreateDbContext()
    {
        if (_dbContext is not null)
        {
            return _dbContext;
        }

        _dbContext = poolService.GetNewDbContext();

        return _dbContext;
    }
}
