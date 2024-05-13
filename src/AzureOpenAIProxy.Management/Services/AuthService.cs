using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Services;

public class AuthService(AuthenticationStateProvider authenticationStateProvider, IDbContextFactory<AoaiProxyContext> dbContextFactory) : IAuthService, IDisposable
{
    AoaiProxyContext db = dbContextFactory.CreateDbContext();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            db.Dispose();
        }
    }

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
