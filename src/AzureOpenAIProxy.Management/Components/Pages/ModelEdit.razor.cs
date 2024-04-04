using System.Data;
using System.Data.Common;
using AzureOpenAIProxy.Management.Components.ModelManagement;
using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class ModelEdit : ComponentBase
{
    [Parameter]
    public required string Id { get; set; }

    [Inject]
    public IModelService ModelService { get; set; } = null!;

    [Inject]
    public AoaiProxyContext DbContext { get; set; } = null!;

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

        OwnerCatalog m = new()
        {
            CatalogId = Guid.Parse(Id),
            FriendlyName = model.FriendlyName!,
            DeploymentName = model.DeploymentName!,
            EndpointKey = model.EndpointKey!,
            EndpointUrl = model.EndpointUrl!,
            ModelType = model.ModelType!.Value,
            Location = model.Location!,
            Active = model.Active,
        };

        await ModelService.UpdateOwnerCatalogAsync(m.CatalogId, m);

        NavigationManager.NavigateTo("/models");
    }
}
