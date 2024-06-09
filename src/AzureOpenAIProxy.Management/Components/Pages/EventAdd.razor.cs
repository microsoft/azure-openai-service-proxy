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
    public required IAuthService AuthService { get; set; }

    [Inject]
    public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    [Inject]
    public IModelService ModelService { get; set; } = null!;

    public EventEditorModel Model { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Model.AvailableModels = await ModelService.GetOwnerCatalogsAsync();
        (Model.OrganizerEmail, Model.OrganizerName) = await AuthService.GetCurrentUserEmailNameAsync();
        Model.Start = DateTime.Today.AddHours(-12); // allow for tz relative to utc
        Model.End = DateTime.Today.AddDays(7).AddHours(12); // set a sensible 1 week from today default
    }

    public async Task HandleValidSubmit(EventEditorModel model)
    {
        Event? newEvent = await EventService.CreateEventAsync(model);

        if (model.SelectedModels is not null && newEvent is not null)
        {
            await EventService.UpdateModelsForEventAsync(newEvent.EventId, model.SelectedModels.ToList().Select(Guid.Parse));
        }

        NavigationManager.NavigateTo("/events");
    }
}
