namespace AzureAIProxy.Shared.Database;

public partial class Assistant
{
    public string ApiKey { get; set; } = null!;

    public AssistantIdType IdType { get; set; }

    public string Id { get; set; } = null!;
}
