namespace AzureOpenAIProxy.Management.Database;

public partial class OwnerEventMap
{
    public Guid OwnerId { get; set; }

    public string EventId { get; set; } = null!;

    public bool Creator { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Owner Owner { get; set; } = null!;
}
