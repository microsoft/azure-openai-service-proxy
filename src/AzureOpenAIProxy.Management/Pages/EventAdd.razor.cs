using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Pages;

public partial class EventAdd : ComponentBase
{
    [Inject]
    public required AoaiProxyContext DbContext { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    public Event Event { get; set; } = new Event();

    public async Task HandleValidSubmit()
    {
        await DbContext.CreateEventAsync(Event, Guid.NewGuid());
        NavigationManager.NavigateTo("/events");
    }
}
