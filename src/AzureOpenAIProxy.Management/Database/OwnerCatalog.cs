using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureOpenAIProxy.Management.Database;

public partial class OwnerCatalog
{
    public string OwnerId { get; set; } = null!;

    public Guid CatalogId { get; set; }

    public string DeploymentName { get; set; } = null!;

    [NotMapped]
    public string EndpointUrl { get; set; } = null!;

    [NotMapped]
    public string EndpointKey { get; set; } = null!;

    public bool Active { get; set; }

    public ModelType? ModelType { get; set; }

    public string Location { get; set; } = null!;

    public string FriendlyName { get; set; } = null!;

    public byte[]? EndpointUrlEncrypted { get; set; }

    public byte[]? EndpointKeyEncrypted { get; set; }

    public virtual Owner Owner { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
