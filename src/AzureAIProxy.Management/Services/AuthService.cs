using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AzureAIProxy.Management.Services;

public class AuthService(AuthenticationStateProvider authenticationStateProvider, AoaiProxyContext db) : IAuthService
{
    public async Task<string> GetCurrentUserEntraIdAsync()
    {
        AuthenticationState authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        string entraId = authState.User.GetEntraId();
        return entraId;
    }

    public async Task<(string email, string name)> GetCurrentUserEmailNameAsync()
    {
        string entraId = await GetCurrentUserEntraIdAsync();
        var owner = await db.Owners
                             .Where(o => o.OwnerId == entraId)
                             .Select(o => new { o.Name, o.Email })
                             .FirstOrDefaultAsync();

        return (owner?.Email ?? string.Empty, owner?.Name ?? string.Empty);
    }
}
