using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Proxy.NET.Services;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<CustomAuthenticationOptions> options,
    IAuthorizeService authorizeService,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<CustomAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var request = Context.Request;
        var apiKey = request.Headers["api-key"].FirstOrDefault();

        if (string.IsNullOrEmpty(apiKey))
            return AuthenticateResult.Fail("Authentication failed.");

        Context.Items["RequestContext"] = await authorizeService.GetRequestContextByApiKey(apiKey);

        var identity = new ClaimsIdentity(null, nameof(ApiKeyAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
