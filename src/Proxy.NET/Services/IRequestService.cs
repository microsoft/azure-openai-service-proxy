namespace Proxy.NET.Services;

public interface IRequestService
{
    Task GenUserContext(HttpContext context);
    object? GetUserContext(HttpContext context);
}
