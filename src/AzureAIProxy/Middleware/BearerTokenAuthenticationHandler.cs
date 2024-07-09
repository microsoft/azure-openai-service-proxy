using System.Security.Claims;
using System.Text.Encodings.Web;
using AzureAIProxy.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AzureAIProxy.Middleware;

public class BearerTokenAuthenticationHandler(
    IOptionsMonitor<ProxyAuthenticationOptions> options,
    IAuthorizeService authorizeService,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<ProxyAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var bearerTokenValues))
            return AuthenticateResult.Fail("Missing Bearer Token.");

        var bearerToken = bearerTokenValues.ToString(); // Convert StringValues to string
        var bearerTokenParts = bearerToken.Split(' ');
        if (
            bearerTokenParts.Length != 2
            || !bearerTokenParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase)
        )
            return AuthenticateResult.Fail("Invalid Bearer Token.");

        var token = bearerTokenParts[1];

        if (string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.Fail("Missing Bearer Token.");

        var requestContext = await authorizeService.IsUserAuthorizedAsync(token);
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
