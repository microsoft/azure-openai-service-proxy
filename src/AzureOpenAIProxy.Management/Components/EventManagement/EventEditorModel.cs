using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace AzureOpenAIProxy.Management.Components.EventManagement;

public class EventEditorModel
{
    [Required]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Url text is required")]
    [StringLength(256)]
    public string? UrlText { get; set; }
    [Required(ErrorMessage = "Url is required")]
    [StringLength(256)]
    [Url]
    public string? Url { get; set; }

    [StringLength(256)]
    [Url]
    public string? EventImageUrl { get; set; }
    
    [Required]
    public string? Description { get; set; }
    [Required]
    public DateTime? Start { get; set; }
    [Required]
    public int TimeZoneOffset { get; set; } = 0;
    [Required]
    public string? TimeZoneLabel { get; set; } = "UTC";
    [Required]
    public DateTime? End { get; set; }
    [Required(ErrorMessage = "Organizer name is required")]
    [StringLength(128)]
    public string? OrganizerName { get; set; }
    [Required(ErrorMessage = "Organizer email is required")]
    [StringLength(128)]
    [EmailAddress]
    public string? OrganizerEmail { get; set; }
    [Required(ErrorMessage = "Specify the maximum number of tokens allowed per request")]
    public int MaxTokenCap { get; set; } = 4096;
    [Required(ErrorMessage = "Specify the maximum number of requests allowed per day per token")]
    public int DailyRequestCap { get; set; } = 2000;
    public bool Active { get; set; }

    [Required(ErrorMessage = "Time zone is required")]
    public TimeZoneInfo? SelectedTimeZone { get; set; }
}
