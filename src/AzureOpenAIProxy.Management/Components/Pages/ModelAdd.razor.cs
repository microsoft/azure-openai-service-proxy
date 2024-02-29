using AzureOpenAIProxy.Management.Components.ModelManagement;
using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class ModelAdd : ComponentBase
{
    [Inject]
    public required IModelService ModelService { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    public async Task HandleValidSubmit(ModelEditorModel model)
    {
        OwnerCatalog _ = await ModelService.AddOwnerCatalogAsync(model);

        // NavigationManager.NavigateTo("/models", forceLoad: true);
        NavigationManager.NavigateTo("/models");
    }
}
