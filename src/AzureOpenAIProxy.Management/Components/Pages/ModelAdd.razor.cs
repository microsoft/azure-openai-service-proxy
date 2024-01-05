using AzureOpenAIProxy.Management.Components.ModelManagement;
using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class ModelAdd : ComponentBase
{
    [Inject]
    public required AoaiProxyContext DbContext { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    public async Task HandleValidSubmit(ModelEditorModel model)
    {
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        string entraId = authState.User.GetEntraId();

        Owner owner = await DbContext.Owners.FirstAsync(o => o.EntraId == entraId);

        OwnerCatalog catalog = new()
        {
            Owner = owner,
            Active = model.Active,
            DeploymentName = model.DeploymentName!,
            EndpointKey = model.EndpointKey!,
            ModelType = model.ModelType,
            ResourceName = model.ResourceName!
        };

        await DbContext.OwnerCatalogs.AddAsync(catalog);
        await DbContext.SaveChangesAsync();

        NavigationManager.NavigateTo("/models", forceLoad: true);
    }
}
