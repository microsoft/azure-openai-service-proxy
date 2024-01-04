using System.ComponentModel.DataAnnotations;
using AzureOpenAIProxy.Management.Database;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventAdd : ComponentBase
{
    [Inject]
    public required AoaiProxyContext DbContext { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [SupplyParameterFromForm]
    public NewEventModel? Model { get; set; }

    protected override void OnInitialized() => Model ??= new();

    public async Task HandleValidSubmit()
    {
        // await DbContext.CreateEventAsync(Model!, Guid.NewGuid());
        // NavigationManager.NavigateTo("/events");
    }

    public class NewEventModel
    {
        [Required]
        [StringLength(256)]
        [Url]
        public string UrlText { get; set; } = null!;
        [Required]
        [StringLength(256)]
        public string Url { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
        [Required]
        public DateTimeOffset Start { get; set; }
        [Required]
        public DateTimeOffset End { get; set; }
        [Required]
        [StringLength(128)]
        public string OrganizerName { get; set; } = null!;
        [Required]
        [StringLength(128)]
        [EmailAddress]
        public string OrganizerEmail { get; set; } = null!;
        [Required]
        public int MaxTokenCap { get; set; }
        [Required]
        public bool SingleCode { get; set; }
        [Required]
        public int DailyRequestCap { get; set; }
        public bool Active { get; set; } = false;
    }
}
