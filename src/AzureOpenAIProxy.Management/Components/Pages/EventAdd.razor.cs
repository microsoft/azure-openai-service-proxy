using AzureOpenAIProxy.Management.Components.EventManagement;
using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventAdd : ComponentBase
{
    [Inject]
    public required IEventService EventService { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    public async Task HandleValidSubmit(EventEditorModel model)
    {
        await EventService.CreateEventAsync(model);
        NavigationManager.NavigateTo("/events", forceLoad: true);
    }
}
