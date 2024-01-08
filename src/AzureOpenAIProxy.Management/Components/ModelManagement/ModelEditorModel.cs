using System.ComponentModel.DataAnnotations;
using AzureOpenAIProxy.Management.Database;

namespace AzureOpenAIProxy.Management.Components.ModelManagement;

public class ModelEditorModel
{
    [Required]
    public string? DeploymentName { get; set; }

    [Required]
    public string? ResourceName { get; set; }

    [Required]
    public string? EndpointKey { get; set; }

    [Required]
    public bool Active { get; set; }

    [Required]
    public ModelType? ModelType { get; set; } = null;
}
