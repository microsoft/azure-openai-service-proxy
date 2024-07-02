using Proxy.NET.Models;

namespace Proxy.NET.Services;

public interface IMetricService
{
    Task LogApiUsageAsync(RequestContext requestContext, string? response);
}
