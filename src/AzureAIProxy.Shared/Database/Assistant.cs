namespace AzureAIProxy.Shared.Database;
using System.ComponentModel.DataAnnotations.Schema;

public enum Scope
{
    Personal,
    Global
}

public partial class Assistant
{
    public string ApiKey { get; set; } = null!;
    public string Id { get; set; } = null!;

    [NotMapped]
    public Scope Scope { get; set; } = Scope.Personal; // Default to Personal
}
