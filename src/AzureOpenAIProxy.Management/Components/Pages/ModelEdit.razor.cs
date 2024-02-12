using AzureOpenAIProxy.Management.Components.ModelManagement;
using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class ModelEdit : ComponentBase
{
    [Parameter]
    public required string Id { get; set; }

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
        OwnerCatalog? m = await DbContext.OwnerCatalogs.FindAsync(Guid.Parse(Id));

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
            ModelType = m.ModelType,
            EndpointUrl = m.EndpointUrl,
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

        await DbContext.SaveChangesAsync();

        NavigationManager.NavigateTo("/models");
    }
}
