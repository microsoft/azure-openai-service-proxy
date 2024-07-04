using Microsoft.AspNetCore.Authentication;

namespace Proxy.NET.Authentication;

public class ProxyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string ApiKeyScheme = "ApiKeyScheme";
    public const string JwtScheme = "JwtScheme";
}
