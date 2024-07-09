namespace AzureAIProxy.Models;

public partial class Deployment
{
    public string EndpointKey { get; set; } = null!;
    public string DeploymentName { get; set; } = null!;
    public string ModelType { get; set; } = null!;
    public string EndpointUrl { get; set; } = null!;
    public Guid CatalogId { get; set; } = Guid.Empty;
    public string Location { get; set; } = null!;
}
