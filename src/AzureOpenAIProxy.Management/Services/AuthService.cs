using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Services;

public class AuthService(AuthenticationStateProvider authenticationStateProvider) : IAuthService
{
    public async Task<string> GetCurrentUserEntraIdAsync()
    {
        AuthenticationState authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        string entraId = authState.User.GetEntraId();
        return entraId;
    }
}
