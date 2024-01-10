using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventList : ComponentBase
{
    [Inject]
    public required IEventService EventService { get; set; }

    public IEnumerable<Event>? Events { get; set; }

    protected override async Task OnInitializedAsync() => Events = await EventService.GetOwnerEventsAsync();
}
