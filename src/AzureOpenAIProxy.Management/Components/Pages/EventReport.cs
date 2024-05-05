// using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;
// using MudBlazor;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventReport
{
    [Inject]
    private IEventService EventService { get; set; } = null!;

    [Inject]
    public required IConfiguration Configuration { get; set; }

    private List<AllEvents>? AllEvents { get; set; }

    private int TotalRegistations { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AllEvents = await EventService.GetAllEventsAsync();
        // calculate total attendees
        TotalRegistations = AllEvents.Sum(e => e.Registered);
    }
}
