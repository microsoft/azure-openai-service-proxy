using AzureOpenAIProxy.Management.Components.ModelManagement;
using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class ModelDelete : ComponentBase
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

        // check if the catalogId is in EventCatalogMap
        var eventMap = await DbContext.Events.FirstOrDefaultAsync(x => x.Catalogs.Any(c => c.CatalogId == m.CatalogId));

        // check if the catalogId is in Metric
        var metric = await DbContext.Metrics.FirstOrDefaultAsync(x => x.CatalogId == m.CatalogId);

        if (eventMap is null && metric is null)
        {
            DbContext.OwnerCatalogs.Remove(m);

            // This will exception if the catalog_id is in use in the metric table
            try { await DbContext.SaveChangesAsync(); }
            catch (Exception ex)
            {}
        }

        NavigationManager.NavigateTo("/models");
    }
}
