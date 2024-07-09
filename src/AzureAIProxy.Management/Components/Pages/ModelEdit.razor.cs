using System.Data;
using AzureAIProxy.Management.Components.ModelManagement;
using AzureAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace AzureAIProxy.Management.Components.Pages;

public partial class ModelEdit : ComponentBase
{
    [Parameter]
    public required string Id { get; set; }

    [Inject]
    public IModelService ModelService { get; set; } = null!;

    [Inject]
    public AzureAIProxyDbContext DbContext { get; set; } = null!;

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
        OwnerCatalog? m = await DbContext.OwnerCatalogs.FindAsync(Guid.Parse(Id));

        if (m is null)
        {
            NavigationManager.NavigateTo("/models");
            return;
        }

        m.FriendlyName = model.FriendlyName!;
        m.DeploymentName = model.DeploymentName!;
        m.EndpointKey = model.EndpointKey!;
        m.ModelType = model.ModelType!.Value;
        m.EndpointUrl = model.EndpointUrl!;
        m.Location = model.Location!;
        m.Active = model.Active;

        await ModelService.UpdateOwnerCatalogAsync(m);

        NavigationManager.NavigateTo("/models");
    }
}
