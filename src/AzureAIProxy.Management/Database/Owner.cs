using System;
using System.Collections.Generic;

namespace AzureAIProxy.Management.Database;

public partial class Owner
{
    public string OwnerId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<OwnerCatalog> OwnerCatalogs { get; set; } = new List<OwnerCatalog>();

    public virtual ICollection<OwnerEventMap> OwnerEventMaps { get; set; } = new List<OwnerEventMap>();
}
