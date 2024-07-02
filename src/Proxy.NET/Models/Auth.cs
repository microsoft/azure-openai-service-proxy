namespace Proxy.NET.Models;

/// <summary>
/// Represents the authentication mechanism used for API requests.
/// </summary>
public class Auth(Auth.Type authType)
{
    public enum Type
    {
        ApiKey, // api-key in header
        Jwt, // https://en.wikipedia.org/wiki/JSON_Web_Token
        None // No authorization required
    }

    public Type AuthType { get; } = authType;
}
