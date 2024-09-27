using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using AzureAIProxy.Services;

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
        if (!Request.Headers.TryGetValue("Authorization", out var apiKeyValues))
            return AuthenticateResult.Fail("Missing API key is empty.");

        // Extract the API key from the Authorization header
        var apiKey = apiKeyValues.ToString().Split(" ").Last(); // Convert StringValues to string
        if (string.IsNullOrWhiteSpace(apiKey))
            return AuthenticateResult.Fail("API key is empty.");

        var requestContext = await authorizeService.IsUserAuthorizedAsync(apiKey);
        if (requestContext is null)
            return AuthenticateResult.Fail("Authentication failed.");

        Context.Items["RequestContext"] = requestContext;

        var identity = new ClaimsIdentity(null, nameof(BearerTokenAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
