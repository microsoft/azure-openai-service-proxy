using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Pages;

public partial class EventList : ComponentBase
{
    [Inject]
    public required AoaiProxyContext DbContext { get; set; }

    public IEnumerable<Event>? Events { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Events = await DbContext.Events.ToListAsync();
    }
}
