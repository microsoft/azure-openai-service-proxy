using System;
using System.Collections.Generic;

namespace AzureOpenAIProxy.Management.Database;

public partial class Metric
{
    public string EventId { get; set; } = null!;

    public string ApiKey { get; set; } = null!;

    public DateOnly DateStamp { get; set; }

    public TimeOnly TimeStamp { get; set; }

    public string Resource { get; set; } = null!;

    public string Usage { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;
}
