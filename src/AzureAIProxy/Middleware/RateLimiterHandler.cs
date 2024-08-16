using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Middleware;

public class RateLimiterHandler(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        RequestContext? requestContext = context.Items["RequestContext"] as RequestContext;

        if (requestContext is not null && requestContext.RateLimitExceed)
        {
            await OpenAIErrorResponse.TooManyRequests(
                $"The event daily request rate of {requestContext.DailyRequestCap} calls has been exceeded. Requests are disabled until UTC midnight."
            ).WriteAsync(context);
        }
        else
        {
            await next(context);
        }
    }
}
