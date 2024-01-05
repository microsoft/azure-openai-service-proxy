using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AzureOpenAIProxy.Management.Components.EventManagement;

public partial class EventEditor : ComponentBase
{
    [Parameter]
    public EventEditorModel Model { get; set; } = null!;

    [Parameter]
    public Func<EventEditorModel, Task> OnValidSubmit { get; set; } = null!;

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

        isSubmitting = true;
        await OnValidSubmit(Model);
        isSubmitting = false;
    }

    public class EventEditorModel
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
