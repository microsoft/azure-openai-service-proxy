using Microsoft.AspNetCore.Authentication;

namespace Proxy.NET.Middleware;

public class ProxyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string ApiKeyScheme = "ApiKeyScheme";
    public const string JwtScheme = "JwtScheme";
}
