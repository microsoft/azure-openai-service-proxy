namespace AzureAIProxy.Middleware;

public class RateLimiterHandler(RequestDelegate next)
{
    private const int RateLimitStatusCode = 429;
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Items.TryGetValue("RateLimited", out var rateLimit) && rateLimit is true)
        {
            var dailyRequestCap = context.Items["DailyRequestCap"] ?? 0;
            context.Response.StatusCode = RateLimitStatusCode; // Too Many Requests
            await context.Response.WriteAsJsonAsync(
                new
                {
                    code = RateLimitStatusCode,
                    message = $"The event daily request rate of {dailyRequestCap} calls has been exceeded. Requests are disabled until UTC midnight."
                }
            );
        }
        else
        {
            await _next(context);
        }
    }
}
