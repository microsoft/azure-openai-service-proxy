namespace AzureOpenAIProxy.Management.Services;

public interface IPoolService
{
    AoaiProxyContext GetNewDbContext();
}
