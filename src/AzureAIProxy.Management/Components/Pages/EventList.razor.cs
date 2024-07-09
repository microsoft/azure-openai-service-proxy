using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Management.Components.Pages;

public partial class EventList : ComponentBase
{
    [Inject]
    public required IEventService EventService { get; set; }

    [Inject]
    public required IConfiguration Configuration { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    public IEnumerable<Event>? Events { get; set; }

    protected override async Task OnInitializedAsync() => Events = await EventService.GetOwnerEventsAsync();

    private async Task OpenDialog(Event @event)
    {
        DialogParameters<DeleteConfirmation> parameters = new()
        {
            { x => x.ContentText, $"Do you really want to delete the event '{@event.EventCode}'?" },
            { x => x.ButtonText, "Delete" },
            { x => x.Color, Color.Error }
        };
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<DeleteConfirmation>("Delete Record", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await EventService.DeleteEventAsync(@event.EventId);
            Events = await EventService.GetOwnerEventsAsync();
        }
    }
}
