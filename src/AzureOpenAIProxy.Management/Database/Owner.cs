namespace AzureOpenAIProxy.Management.Database;

public partial class Owner
{
    public string EntraId { get; set; } = null!;

    public Guid OwnerId { get; set; }

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<OwnerCatalog> OwnerCatalogs { get; set; } = new List<OwnerCatalog>();

    public virtual ICollection<OwnerEventMap> OwnerEventMaps { get; set; } = new List<OwnerEventMap>();
}
