namespace AzureAIProxy.Services;

public interface IAttendeeService
{
    Task<string> AddAttendeeAsync(string userId, string eventId);
    Task<AttendeeKey?> GetAttendeeKeyAsync(string userId, string eventId);
}
