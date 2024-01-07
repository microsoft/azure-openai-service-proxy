using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventList : ComponentBase
{
    [Inject]
    public required AoaiProxyContext DbContext { get; set; }

    [Inject]
    public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    public IEnumerable<Event>? Events { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        string entraId = authState.User.GetEntraId();
        Events = await DbContext.Events.Where(e => e.OwnerEventMaps.Any(o => o.Owner.EntraId == entraId)).ToListAsync();
    }
}
