using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class ModelList : ComponentBase
{
    [Inject]
    public required IModelService ModelService { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    public IEnumerable<OwnerCatalog>? Models { get; set; }

    protected override async Task OnInitializedAsync() => Models = await ModelService.GetOwnerCatalogsAsync();

    private async Task OpenDialog(OwnerCatalog resource)
    {
        DialogParameters<DeleteConfirmation> parameters = new()
        {
            { x => x.ContentText, $"Do you really want to delete the resource '{resource.FriendlyName}'?" },
            { x => x.ButtonText, "Delete" },
            { x => x.Color, Color.Error }
        };
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<DeleteConfirmation>("Delete Record", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await ModelService.DeleteOwnerCatalogAsync(resource.CatalogId);
            Models = await ModelService.GetOwnerCatalogsAsync();
        }

    }
}
