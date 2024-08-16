using Microsoft.AspNetCore.Authentication;

namespace AzureAIProxy.Middleware;

public class ProxyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string ApiKeyScheme = "ApiKeyScheme";
    public const string JwtScheme = "JwtScheme";
    public const string BearerTokenScheme = "BearerTokenScheme";
}
