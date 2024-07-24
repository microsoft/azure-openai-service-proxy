namespace AzureAIProxy.Aspire.Components;

public class SwaResource(string name, string workingDirectory) : ExecutableResource(name, "swa", workingDirectory)
{
    internal IResourceBuilder<IResourceWithEndpoints>? AppResource { get; set; }
    internal IResourceBuilder<IResourceWithEndpoints>? ApiResource { get; set; }

    internal string? AppEndpoint => AppResource?.Resource.GetEndpoint("http").Url;
    internal string? ApiEndpoint => ApiResource?.Resource.GetEndpoint("http").Url;
}
