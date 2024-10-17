namespace AzureAIProxy.Shared.Database;
using System.ComponentModel.DataAnnotations.Schema;

public partial class Assistant
{
    public string ApiKey { get; set; } = null!;
    public string Id { get; set; } = null!;
}
