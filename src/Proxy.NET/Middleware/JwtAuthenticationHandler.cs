using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Proxy.NET.Services;

namespace Proxy.NET.Middleware;

public class JwtAuthenticationHandler(
    IOptionsMonitor<ProxyAuthenticationOptions> options,
    IAuthorizeService authorizeService,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<ProxyAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("x-ms-client-principal", out var jwtValues))
            return AuthenticateResult.Fail("Authentication failed.");

        string? jwt = jwtValues.ToString(); // Convert StringValues to string
        if (string.IsNullOrWhiteSpace(jwt))
            return AuthenticateResult.Fail("JWT token is empty.");

        var requestContext = await authorizeService.GetRequestContextFromJwtAsync(jwt);
        if (requestContext is null)
            return AuthenticateResult.Fail("Authentication failed.");

        Context.Items["RequestContext"] = requestContext;

        var identity = new ClaimsIdentity(null, nameof(ApiKeyAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
