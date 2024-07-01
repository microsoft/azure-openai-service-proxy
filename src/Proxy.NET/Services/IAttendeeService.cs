namespace Proxy.NET.Services;

public interface IAttendeeService
{
    Task<string> AddAttendeeAsync(string userId, string eventId);
    Task<(string apiKey, bool active)> GetAttendeeKeyAsync(string userId, string eventId);
}
