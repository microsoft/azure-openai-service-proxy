using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Proxy.NET.Authentication;

public class JwtAuthenticationHandler(IOptionsMonitor<ProxyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<ProxyAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Implement JWT authentication logic here
        // This method validates the JWT token and produces a ClaimsPrincipal if valid
        return Task.FromResult(AuthenticateResult.Fail("Not implemented yet"));
    }
}
