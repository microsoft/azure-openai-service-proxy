using System.Data;
using AzureOpenAIProxy.Management.Components.ModelManagement;
using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class ModelEdit : ComponentBase
{
    [Parameter]
    public required string Id { get; set; }

    [Inject]
    public IModelService ModelService { get; set; } = null!;

    [Inject]
    public IDbContextFactory<AoaiProxyContext> DbContextFactory { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    public ModelEditorModel Model { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(Id))
        {
            NavigationManager.NavigateTo("/models");
            return;
        }

        OwnerCatalog? m = await ModelService.GetOwnerCatalogAsync(Guid.Parse(Id));

        if (m is null)
        {
            NavigationManager.NavigateTo("/models");
            return;
        }

        Model = new()
        {
            FriendlyName = m.FriendlyName,
            DeploymentName = m.DeploymentName,
            EndpointKey = m.EndpointKey,
            EndpointUrl = m.EndpointUrl,
            ModelType = m.ModelType,
            Location = m.Location,
            Active = m.Active,
        };
    }

    private async Task OnValidSubmit(ModelEditorModel model)
    {
        await ModelService.UpdateOwnerCatalogAsync(model, Guid.Parse(Id));
        NavigationManager.NavigateTo("/models");
    }
}
