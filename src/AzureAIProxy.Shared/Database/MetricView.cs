namespace AzureAIProxy.Shared.Database;

public partial class MetricView
{
    public string EventId { get; set; } = null!;

    public string Resource { get; set; } = null!;

    public DateTime DateStamp { get; set; }

    public TimeOnly TimeStamp { get; set; }

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }
}
