using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Proxy.NET.Services;

namespace Proxy.NET.Middleware;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ProxyAuthenticationOptions> options,
    IAuthorizeService authorizeService,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<ProxyAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("api-key", out var apiKeyValues))
            return AuthenticateResult.Fail("Missing API key is empty.");

        var apiKey = apiKeyValues.ToString(); // Convert StringValues to string
        if (string.IsNullOrWhiteSpace(apiKey))
            return AuthenticateResult.Fail("Missing API key is empty.");

        var requestContext = await authorizeService.IsUserAuthorizedAsync(apiKey);
        if (requestContext is null)
            return AuthenticateResult.Fail("Authentication failed.");

        Context.Items["RequestContext"] = requestContext;
        Context.Items["RateLimited"] = requestContext.RateLimitExceed;
        Context.Items["DailyRequestCap"] = requestContext.DailyRequestCap;

        var identity = new ClaimsIdentity(null, nameof(ApiKeyAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
