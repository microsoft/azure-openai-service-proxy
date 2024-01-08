using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class ModelList : ComponentBase
{
    [Inject]
    public required AoaiProxyContext DbContext { get; set; }

    [Inject]
    public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    public IEnumerable<OwnerCatalog>? Models { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        string entraId = authState.User.GetEntraId();
        Models = await DbContext.OwnerCatalogs.Where(e => e.Owner.EntraId == entraId).ToListAsync();
    }
}
