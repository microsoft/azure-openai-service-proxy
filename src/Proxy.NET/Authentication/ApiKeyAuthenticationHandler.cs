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
        var apiKey = Request.Headers["api-key"].FirstOrDefault();

        if (string.IsNullOrEmpty(apiKey))
            return AuthenticateResult.Fail("Authentication failed.");

        var requestContext = await authorizeService.IsUserAuthorized(apiKey);
        if (requestContext == null)
            return AuthenticateResult.Fail("Authentication failed.");

        Context.Items["RequestContext"] = requestContext;

        var identity = new ClaimsIdentity(null, nameof(ApiKeyAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
