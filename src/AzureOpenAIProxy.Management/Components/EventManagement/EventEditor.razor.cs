using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AzureOpenAIProxy.Management.Components.EventManagement;

public partial class EventEditor : ComponentBase
{
    [Parameter]
    public EventEditorModel Model { get; set; } = null!;

    [Parameter]
    public EventCallback<EventEditorModel> ModelChanged { get; set; }

    [Parameter]
    public EventCallback<EventEditorModel> OnValidSubmit { get; set; }

    private EditContext? editContext;

    private ValidationMessageStore? messageStore;

    private bool isSubmitting = false;

    private IEnumerable<TimeZoneInfo>? TimeZones { get; set; }

    protected override Task OnInitializedAsync()
    {
        Model ??= new();
        editContext = new(Model);
        editContext.OnValidationRequested += EditContext_OnValidationRequested;
        messageStore = new(editContext);
        TimeZones = TimeZoneInfo.GetSystemTimeZones();
        return Task.CompletedTask;
    }

    private void EditContext_OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        if (Model is not null && editContext is not null && messageStore is not null)
        {
            messageStore.Clear();
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
        await OnValidSubmit.InvokeAsync(Model);
        isSubmitting = false;
    }

    private Task<IEnumerable<TimeZoneInfo>> FilterTimezones(string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return Task.FromResult(TimeZones!);
        }

        return Task.FromResult(TimeZones!.Where(tz => tz.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase)));
    }
}
