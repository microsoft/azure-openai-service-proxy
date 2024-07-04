using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Proxy.NET.Services;

namespace Proxy.NET.Authentication;

public class JwtAuthenticationHandler(
    IOptionsMonitor<ProxyAuthenticationOptions> options,
    IAuthorizeService authorizeService,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<ProxyAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var jwt = Request.Headers["x-ms-client-principal"].FirstOrDefault();
        if (string.IsNullOrEmpty(jwt))
            return AuthenticateResult.Fail("Authentication failed.");

        var requestContext = authorizeService.GetRequestContextFromJwt(jwt);
        if (requestContext == null)
            return AuthenticateResult.Fail("Authentication failed.");

        Context.Items["RequestContext"] = requestContext;

        var identity = new ClaimsIdentity(null, nameof(ApiKeyAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        // Awaiting a completed task to suppress the warning
        await Task.CompletedTask;

        return AuthenticateResult.Success(ticket);
    }
}
