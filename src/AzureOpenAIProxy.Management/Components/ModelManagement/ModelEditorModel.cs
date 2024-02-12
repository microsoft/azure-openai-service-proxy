using System.ComponentModel.DataAnnotations;
using AzureOpenAIProxy.Management.Database;

namespace AzureOpenAIProxy.Management.Components.ModelManagement;

public class ModelEditorModel
{
    [Required]
    [StringLength(64)]
    public string? FriendlyName { get; set; }

    [Required]
    [StringLength(64)]
    public string? DeploymentName { get; set; }

    [Required]
    [StringLength(256)]
    public string? EndpointUrl { get; set; }

    [Required]
    [StringLength(128)]
    public string? EndpointKey { get; set; }

    [Required]
    [StringLength(64)]
    public string? Location { get; set; }

    [Required]
    public bool Active { get; set; }

    [Required]
    public ModelType? ModelType { get; set; } = null;
}
