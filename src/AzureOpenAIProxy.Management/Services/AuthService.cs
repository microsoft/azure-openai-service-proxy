using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Services;

public class AuthService(AuthenticationStateProvider authenticationStateProvider, AoaiProxyContext db) : IAuthService
{
    public async Task<Owner> GetCurrentOwnerAsync()
    {
        string entraId = await GetCurrentUserEntraIdAsync();
        return await db.Owners.FirstOrDefaultAsync(o => o.OwnerId == entraId) ?? throw new InvalidOperationException("EntraID is not a registered owner.");
    }

    public async Task<string> GetCurrentUserEntraIdAsync()
    {
        AuthenticationState authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        string entraId = authState.User.GetEntraId();
        return entraId;
    }
}
