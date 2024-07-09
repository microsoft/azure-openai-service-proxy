using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace AzureAIProxy.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
[DebuggerDisplay("{ToString(),nq}")]
public class BearerTokenAuthorizeAttribute : AuthorizeAttribute
{
    public BearerTokenAuthorizeAttribute() => AuthenticationSchemes = ProxyAuthenticationOptions.BearerTokenScheme;
}
