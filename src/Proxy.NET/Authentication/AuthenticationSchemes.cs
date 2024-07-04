using Microsoft.AspNetCore.Authentication;

public class CustomAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string ApiKeyScheme = "ApiKeyScheme";
    public const string JwtScheme = "JwtScheme";
}
