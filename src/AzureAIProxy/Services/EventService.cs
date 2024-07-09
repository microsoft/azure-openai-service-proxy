using System.Data;
using AzureAIProxy.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AzureAIProxy.Models;

namespace AzureAIProxy.Services;

public class EventService(AzureAIProxyContext db, IMemoryCache memoryCache) : IEventService
{
    /// <summary>
    /// Retrieves the registration information for an event with the specified event ID.
    /// </summary>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the event registration information, or null if no registration is found.</returns>
    public async Task<EventRegistration?> GetEventRegistrationInfoAsync(string eventId)
    {
        if (memoryCache.TryGetValue(eventId, out EventRegistration? cachedContext))
            return cachedContext;

        var result = await db
            .Events.Where(e => e.EventId == eventId)
            .Select(e => new EventRegistration
            {
                EventId = e.EventId,
                EventCode = e.EventCode,
                EventImageUrl = e.EventImageUrl,
                OrganizerName = e.OrganizerName,
                OrganizerEmail = e.OrganizerEmail,
                EventMarkdown = e.EventMarkdown,
                StartTimestamp = e.StartTimestamp,
                EndTimestamp = e.EndTimestamp,
                TimeZoneOffset = e.TimeZoneOffset,
                TimeZoneLabel = e.TimeZoneLabel
            })
            .FirstOrDefaultAsync();

        memoryCache.Set(eventId, result, TimeSpan.FromMinutes(1));
        return result;
    }
}
