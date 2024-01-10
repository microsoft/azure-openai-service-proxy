namespace AzureOpenAIProxy.Management.Database;

public partial class OwnerCatalog
{
    public string OwnerId { get; set; } = null!;

    public Guid CatalogId { get; set; }

    public string DeploymentName { get; set; } = null!;

    public string ResourceName { get; set; } = null!;

    public string EndpointKey { get; set; } = null!;

    public bool Active { get; set; }

    public ModelType? ModelType { get; set; }

    public virtual Owner Owner { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
