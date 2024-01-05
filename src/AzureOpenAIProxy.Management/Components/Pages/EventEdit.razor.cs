using AzureOpenAIProxy.Management.Components.EventManagement;
using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventEdit : ComponentBase
{
    [Parameter]
    public string Id { get; set; } = string.Empty;

    [Inject]
    public AoaiProxyContext DbContext { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    public EventEditor.EventEditorModel Model { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(Id))
        {
            NavigationManager.NavigateTo("/events");
            return;
        }
        Event? evt = await DbContext.Events.FindAsync(Id);

        if (evt is null)
        {
            NavigationManager.NavigateTo("/events");
            return;
        }

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

    private async Task OnValidSubmit(EventEditor.EventEditorModel model)
    {
        Event? evt = await DbContext.Events.FindAsync(Id);

        if (evt is null)
        {
            NavigationManager.NavigateTo("/events");
            return;
        }

        evt.EventCode = model.Name!;
        evt.EventMarkdown = model.Description!;
        evt.StartUtc = model.Start!.Value;
        evt.EndUtc = model.End!.Value;
        evt.EventUrl = model.Url!;
        evt.EventUrlText = model.UrlText!;
        evt.OrganizerEmail = model.OrganizerEmail!;
        evt.OrganizerName = model.OrganizerName!;
        evt.Active = model.Active;
        evt.MaxTokenCap = model.MaxTokenCap;
        evt.DailyRequestCap = model.DailyRequestCap;
        evt.SingleCode = model.SingleCode;

        await DbContext.SaveChangesAsync();

        NavigationManager.NavigateTo("/events");
    }
}
