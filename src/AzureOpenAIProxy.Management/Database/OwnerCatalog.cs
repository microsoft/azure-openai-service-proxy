namespace AzureOpenAIProxy.Management.Database;

public partial class OwnerCatalog
{
    public string OwnerId { get; set; } = null!;

    public Guid CatalogId { get; set; }

    public string DeploymentName { get; set; } = null!;

    public string EndpointUrl { get; set; } = null!;

    public string EndpointKey { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string? FriendlyName { get; set; }

    public bool Active { get; set; }

    public ModelType? ModelType { get; set; }

    public virtual Owner Owner { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
