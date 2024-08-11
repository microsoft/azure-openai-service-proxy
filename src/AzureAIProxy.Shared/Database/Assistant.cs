namespace AzureAIProxy.Shared.Database;

public partial class Assistant
{
    public string ApiKey { get; set; } = null!;

    public AssistantType Type { get; set; }

    public string Id { get; set; } = null!;
}
