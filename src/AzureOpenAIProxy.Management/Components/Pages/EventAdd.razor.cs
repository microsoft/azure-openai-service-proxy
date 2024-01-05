using System.ComponentModel.DataAnnotations;
using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventAdd : ComponentBase
{
    [Inject]
    public required AoaiProxyContext DbContext { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }


    [SupplyParameterFromForm]
    public NewEventModel? Model { get; set; }

    private EditContext? editContext;

    private ValidationMessageStore? messageStore;

    private bool isSubmitting = false;

    protected override void OnInitialized()
    {
        Model ??= new();
        editContext = new(Model);
        editContext.OnValidationRequested += EditContext_OnValidationRequested;
        messageStore = new(editContext);
    }

    private void EditContext_OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        if (Model is not null && editContext is not null && messageStore is not null)
        {
            if (Model.Start > Model.End)
            {
                messageStore.Add(editContext.Field(nameof(Model.Start)), "Start date must be before end date");
                messageStore.Add(editContext.Field(nameof(Model.End)), "End date must be after start date");
            }
        }
    }

    public async Task HandleValidSubmit()
    {
        if (Model is null)
        {
            return;
        }

        Event evt = new()
        {
            EventCode = Model.Name!,
            EventUrlText = Model.UrlText!,
            EventUrl = Model.Url!,
            EventMarkdown = Model.Description!,
            StartUtc = Model.Start!.Value,
            EndUtc = Model.End!.Value,
            OrganizerName = Model.OrganizerName!,
            OrganizerEmail = Model.OrganizerEmail!,
            MaxTokenCap = Model.MaxTokenCap,
            SingleCode = Model.SingleCode,
            DailyRequestCap = Model.DailyRequestCap,
            Active = Model.Active
        };

        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        isSubmitting = true;
        await DbContext.CreateEventAsync(evt, authState.User.GetEntraId());
        isSubmitting = false;
        NavigationManager.NavigateTo("/events", forceLoad: true);
    }

    public class NewEventModel
    {
        [Required]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Url text is required")]
        [StringLength(256)]
        public string? UrlText { get; set; }
        [Required(ErrorMessage = "Url is required")]
        [StringLength(256)]
        [Url]
        public string? Url { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public DateTime? Start { get; set; }
        [Required]
        public DateTime? End { get; set; }
        [Required(ErrorMessage = "Organizer name is required")]
        [StringLength(128)]
        public string? OrganizerName { get; set; }
        [Required(ErrorMessage = "Organizer email is required")]
        [StringLength(128)]
        [EmailAddress]
        public string? OrganizerEmail { get; set; }
        [Required(ErrorMessage = "Specify the maximum number of tokens allowed per request")]
        public int MaxTokenCap { get; set; } = 4096;
        [Required]
        public bool SingleCode { get; set; }
        [Required(ErrorMessage = "Specify the maximum number of requests allowed per day per token")]
        public int DailyRequestCap { get; set; } = 10000;
        public bool Active { get; set; }
    }
}
