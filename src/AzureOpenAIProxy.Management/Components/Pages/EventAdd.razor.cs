using AzureOpenAIProxy.Management.Components.EventManagement;
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

    [Inject]
    public IModelService ModelService { get; set; } = null!;

    public EventEditorModel Model { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Model.AvailableModels = await ModelService.GetOwnerCatalogsAsync();
    }

    public async Task HandleValidSubmit(EventEditorModel model)
    {
        Event? newEvent = await EventService.CreateEventAsync(model);

        if (model.SelectedModels is not null && newEvent is not null)
        {
            await EventService.UpdateModelsForEventAsync(newEvent.EventId, model.SelectedModels.ToList().Select(Guid.Parse));
        }

        NavigationManager.NavigateTo("/events", forceLoad: true);
    }
}
