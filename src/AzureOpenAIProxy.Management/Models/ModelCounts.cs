namespace AzureOpenAIProxy.Management.Models;

public class ModelCounts
{
    public string? Resource { get; set; }
    public int Count { get; set; }
    public long PromptTokens { get; set; }
    public long CompletionTokens { get; set; }
    public long TotalTokens { get; set; }
}
