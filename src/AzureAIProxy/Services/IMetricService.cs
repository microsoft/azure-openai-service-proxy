using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

public interface IMetricService
{
    Task LogApiUsageAsync(
        RequestContext requestContext,
        Deployment deployment,
        string? responseContent
    );
}
