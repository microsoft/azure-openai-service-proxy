using AzureAIProxy.Models;

namespace AzureAIProxy.Services;

public interface IMetricService
{
    Task LogApiUsageAsync(
        RequestContext requestContext,
        Deployment deployment,
        string? responseContent
    );
}
