namespace AzureAIProxy.Models;

public class RequestHeader(string key, string value)
{
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;
}
