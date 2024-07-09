using Microsoft.AspNetCore.Authorization;
using AzureAIProxy.Middleware;
using System.Diagnostics;

namespace AzureAIProxy;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
[DebuggerDisplay("{ToString(),nq}")]
public class JwtAuthorizeAttribute : AuthorizeAttribute
{
    public JwtAuthorizeAttribute() => AuthenticationSchemes = ProxyAuthenticationOptions.JwtScheme;
}
