namespace Proxy.NET.Services;

public interface IRequestService
{
    Task CreateAsync(HttpContext context);
    object? GetRequestContext();
}
