using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.ModelManagement;

public partial class ModelEditor : ComponentBase
{
    [Parameter]
    public ModelEditorModel? Model { get; set; }

    [Parameter]
    public EventCallback<ModelEditorModel> ModelChanged { get; set; }

    [Parameter]
    public EventCallback<ModelEditorModel> OnValidSubmit { get; set; }

    private bool isSubmitting = false;

    private bool maskKey = true;

    private void ToggleMaskKey() => maskKey = !maskKey;

    protected override Task OnInitializedAsync()
    {
        Model ??= new();
        return Task.CompletedTask;
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
}
