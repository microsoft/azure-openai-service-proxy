using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class ModelList : ComponentBase
{
    [Parameter]
    public required string Id { get; set; }
}
