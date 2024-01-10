using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class ModelList : ComponentBase
{
    [Inject]
    public required IModelService ModelService { get; set; }

    public IEnumerable<OwnerCatalog>? Models { get; set; }

    protected override async Task OnInitializedAsync() => Models = await ModelService.GetOwnerCatalogsAsync();
}
