using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Proxy.NET.Services;

namespace Proxy.NET.Authentication;

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
            return AuthenticateResult.Fail("Authentication failed.");

        var apiKey = apiKeyValues.ToString(); // Convert StringValues to string
        if (string.IsNullOrWhiteSpace(apiKey))
            return AuthenticateResult.Fail("API key is empty.");

        try
        {
            var requestContext = await authorizeService.IsUserAuthorizedAsync(apiKey);
            if (requestContext is null)
                return AuthenticateResult.Fail("Authentication failed.");

            Context.Items["RequestContext"] = requestContext;

            var identity = new ClaimsIdentity(null, nameof(ApiKeyAuthenticationHandler));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (RateLimiteExceededException ex)
        {
            return AuthenticateResult.Fail(ex.Message);
        }
    }
}
