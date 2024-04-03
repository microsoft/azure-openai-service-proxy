using System.ComponentModel.DataAnnotations;

namespace AzureOpenAIProxy.Management.Components.EventManagement;

public class EventEditorModel
{

    [Required(ErrorMessage = "Event name is required")]
    [StringLength(64)]
    public string? Name { get; set; }


    [StringLength(64)]
    [RegularExpression(@"^[a-zA-Z0-9]{5,}$", ErrorMessage = "Event shared code must be alphanumeric, blank or 5 or more characters long.")]
    public string? EventSharedCode { get; set; }

    [StringLength(256)]
    public string? EventImageUrl { get; set; }

    [Required]
    public string? Description { get; set; }

    [Required]
    public DateTime? Start { get; set; }
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
    [Range(1, 100000, ErrorMessage = "Value for MaxTokenCap must be between 1 and 100000")]
    public int MaxTokenCap { get; set; } = 4096;

    [Required(ErrorMessage = "Specify the maximum number of requests allowed per day per token")]
    [Range(1, 100000, ErrorMessage = "Value for the maximum number of requests must be between 1 and 100000.")]
    public int DailyRequestCap { get; set; } = 2000;

    public bool Active { get; set; }

    [Required(ErrorMessage = "Time zone is required")]
    public TimeZoneInfo? SelectedTimeZone { get; set; }
}
