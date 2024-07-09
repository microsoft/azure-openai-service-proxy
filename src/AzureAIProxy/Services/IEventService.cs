using AzureAIProxy.Models;

namespace AzureAIProxy.Services;

public interface IEventService
{
    Task<EventRegistration?> GetEventRegistrationInfoAsync(string eventId);
}
