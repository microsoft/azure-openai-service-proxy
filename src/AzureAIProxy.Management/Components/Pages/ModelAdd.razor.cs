using AzureAIProxy.Management.Components.ModelManagement;
using AzureAIProxy.Management.Database;
using AzureAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;

namespace AzureAIProxy.Management.Components.Pages;

public partial class ModelAdd : ComponentBase
{
    [Inject]
    public required IModelService ModelService { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    public async Task HandleValidSubmit(ModelEditorModel model)
    {
        OwnerCatalog _ = await ModelService.AddOwnerCatalogAsync(model);
        NavigationManager.NavigateTo("/models");
    }
}
