namespace AzureAIProxy.Models;

public class AuthHeader(string key, string value)
{
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;
}
