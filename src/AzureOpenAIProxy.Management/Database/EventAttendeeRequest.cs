﻿using System;
using System.Collections.Generic;

namespace AzureOpenAIProxy.Management.Database;

public partial class EventAttendeeRequest
{
    public string ApiKey { get; set; } = null!;

    public DateOnly DateStamp { get; set; }

    public int RequestCount { get; set; }

    public int TokenCount { get; set; }

    public virtual EventAttendee ApiKeyNavigation { get; set; } = null!;
}
