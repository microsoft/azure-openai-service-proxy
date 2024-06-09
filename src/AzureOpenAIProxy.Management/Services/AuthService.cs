using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Services;

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
        var owner = await (from o in db.Owners
                           where o.OwnerId == entraId
                           select new { o.Name, o.Email }).FirstOrDefaultAsync();

        if (owner != null)
        {
            return (owner.Email, owner.Name);
        }
        return (string.Empty, string.Empty);
    }
}
