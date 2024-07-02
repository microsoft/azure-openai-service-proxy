using Proxy.NET.Models;

namespace Proxy.NET.Services;

public interface IEventService
{
    Task<EventRegistration?> GetEventRegistrationInfoAsync(string eventId);
}
