using AzureOpenAIProxy.Management.Components.EventManagement;
using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventEdit : ComponentBase
{
    [Parameter]
    public string Id { get; set; } = string.Empty;

    [Inject]
    public IEventService EventService { get; set; } = null!;

    [Inject]
    public IModelService ModelService { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    public EventEditorModel Model { get; set; } = new();

    public IEnumerable<OwnerCatalog> CurrentModels { get; set; } = null!;

    public IEnumerable<string> SelectedModels { get; set; } = [];

    public IEnumerable<OwnerCatalog> AvailableModels { get; set; } = [];

    private bool modelsUpdating = false;

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(Id))
        {
            NavigationManager.NavigateTo("/events");
            return;
        }

        Event? evt = await EventService.GetEventAsync(Id);

        if (evt is null)
        {
            NavigationManager.NavigateTo("/events");
            return;
        }

        AvailableModels = await ModelService.GetOwnerCatalogsAsync();
        CurrentModels = evt.Catalogs;
        SelectedModels = CurrentModels.Select(oc => oc.CatalogId.ToString());

        Model.Name = evt.EventCode;
        Model.EventSharedCode = evt.EventSharedCode;
        Model.Description = evt.EventMarkdown;
        Model.Start = evt.StartTimestamp;
        Model.End = evt.EndTimestamp;
        Model.EventImageUrl = evt.EventImageUrl;
        Model.OrganizerEmail = evt.OrganizerEmail;
        Model.OrganizerName = evt.OrganizerName;
        Model.Active = evt.Active;
        Model.MaxTokenCap = evt.MaxTokenCap;
        Model.DailyRequestCap = evt.DailyRequestCap;
        Model.SelectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(evt.TimeZoneLabel);
        Model.SelectedModels = SelectedModels;
        Model.AvailableModels = AvailableModels;
    }

    private async Task OnValidSubmit(EventEditorModel model)
    {
        Event? evt = await EventService.UpdateEventAsync(Id, model);

        if (evt is null)
        {
            // todo - logging
        }

        NavigationManager.NavigateTo("/events");
    }
}
