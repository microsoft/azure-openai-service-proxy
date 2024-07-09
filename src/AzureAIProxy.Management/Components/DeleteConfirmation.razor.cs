using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AzureAIProxy.Management.Components;
public partial class DeleteConfirmation
{
    [CascadingParameter] public required MudDialogInstance MudDialog { get; set; }

    [Parameter] public required string ContentText { get; set; }

    [Parameter] public required string ButtonText { get; set; }

    [Parameter] public Color Color { get; set; }

    void Submit() => MudDialog.Close(DialogResult.Ok(true));
    void Cancel() => MudDialog.Cancel();
}
