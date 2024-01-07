using AzureOpenAIProxy.Management.Components.EventManagement;
using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventAdd : ComponentBase
{
    [Inject]
    public required AoaiProxyContext DbContext { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    public async Task HandleValidSubmit(EventEditorModel model)
    {
        Event evt = new()
        {
            EventCode = model.Name!,
            EventUrlText = model.UrlText!,
            EventUrl = model.Url!,
            EventMarkdown = model.Description!,
            StartUtc = model.Start!.Value,
            EndUtc = model.End!.Value,
            OrganizerName = model.OrganizerName!,
            OrganizerEmail = model.OrganizerEmail!,
            MaxTokenCap = model.MaxTokenCap,
            SingleCode = model.SingleCode,
            DailyRequestCap = model.DailyRequestCap,
            Active = model.Active
        };

        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        await DbContext.CreateEventAsync(evt, authState.User.GetEntraId());
        NavigationManager.NavigateTo("/events", forceLoad: true);
    }
}
