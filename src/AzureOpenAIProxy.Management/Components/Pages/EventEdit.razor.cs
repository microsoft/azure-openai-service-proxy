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

    public EventEditorModel Model { get; set; } = null!;

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

        Model = new()
        {
            Name = evt.EventCode,
            Description = evt.EventMarkdown,
            Start = evt.StartUtc,
            End = evt.EndUtc,
            Url = evt.EventUrl,
            UrlText = evt.EventUrlText,
            OrganizerEmail = evt.OrganizerEmail,
            OrganizerName = evt.OrganizerName,
            Active = evt.Active,
            MaxTokenCap = evt.MaxTokenCap,
            DailyRequestCap = evt.DailyRequestCap,
            SingleCode = evt.SingleCode,
        };
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

    private string SelectedModelsDisplay(List<string> ids) =>
        ids.Count == 0 ? "Select one or more models" : string.Join(", ", AvailableModels.Where(oc => ids.Contains(oc.CatalogId.ToString())).Select(oc => oc.DeploymentName));

    private async Task UpdateModels()
    {
        modelsUpdating = true;
        await EventService.UpdateModelsForEventAsync(Id, SelectedModels.Select(Guid.Parse));
        modelsUpdating = false;
    }
}
