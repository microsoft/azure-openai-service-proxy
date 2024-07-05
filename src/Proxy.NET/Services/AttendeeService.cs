using System.Data;
using System.Net;
using AzureOpenAIProxy.Management;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Proxy.NET.Models;

namespace Proxy.NET.Services;

/// <summary>
/// Represents a service for managing attendees of events.
/// </summary>
public class AttendeeService(AoaiProxyContext db) : IAttendeeService
{
    /// <summary>
    /// Adds an attendee to an event asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>The API key of the attendee.</returns>
    /// <exception cref="NpgsqlException">Thrown when failed to add the attendee.</exception>
    public async Task<string> AddAttendeeAsync(string userId, string eventId)
    {
        var result = await db.Set<AttendeeApiKey>()
            .FromSqlRaw(
                "SELECT * FROM aoai.add_event_attendee(@userId, @eventId)",
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@eventId", eventId)
            )
            .ToListAsync();

        if (result.Count == 0)
            throw new NpgsqlException("Failed to add attendee");

        return result[0].ApiKey.ToString();
    }

    /// <summary>
    /// Retrieves the attendee key and active status for a given user and event.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>A tuple containing the attendee key and active status.</returns>
    public async Task<AttendeeKey?> GetAttendeeKeyAsync(string userId, string eventId)
    {
        var attendee =
            await db
                .EventAttendees.Where(ea => ea.EventId == eventId && ea.UserId == userId)
                .Select(ea => new AttendeeKey(ea.ApiKey, ea.Active))
                .FirstOrDefaultAsync();

        if (attendee is null)
            return null;

        return attendee;
    }
}
