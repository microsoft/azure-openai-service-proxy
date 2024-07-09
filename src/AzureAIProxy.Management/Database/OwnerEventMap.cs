using System;
using System.Collections.Generic;

namespace AzureAIProxy.Management.Database;

public partial class OwnerEventMap
{
    public string OwnerId { get; set; } = null!;

    public string EventId { get; set; } = null!;

    public bool Creator { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Owner Owner { get; set; } = null!;
}
