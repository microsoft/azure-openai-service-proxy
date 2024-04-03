using System.Data;
using System.Data.Common;
using AzureOpenAIProxy.Management.Components.EventManagement;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace AzureOpenAIProxy.Management.Services;

public class EventService(IAuthService authService, AoaiProxyContext db) : IEventService, IDisposable
{
    private readonly DbConnection conn = db.Database.GetDbConnection();

    public async Task<Event?> CreateEventAsync(EventEditorModel model)
    {
        if (string.IsNullOrEmpty(model.EventSharedCode))
        {
            model.EventSharedCode = null;
        }

        if (string.IsNullOrEmpty(model.EventImageUrl))
        {
            model.EventImageUrl = null;
        }

        Event newEvent = new()
        {
            EventCode = model.Name!,
            EventSharedCode = model.EventSharedCode,
            EventImageUrl = model.EventImageUrl!,
            EventMarkdown = model.Description!,
            StartTimestamp = model.Start!.Value,
            EndTimestamp = model.End!.Value,
            TimeZoneOffset = model.SelectedTimeZone!.BaseUtcOffset.Minutes,
            TimeZoneLabel = model.SelectedTimeZone!.Id,
            OrganizerName = model.OrganizerName!,
            OrganizerEmail = model.OrganizerEmail!,
            MaxTokenCap = model.MaxTokenCap,
            DailyRequestCap = model.DailyRequestCap,
            Active = model.Active
        };

        string entraId = await authService.GetCurrentUserEntraIdAsync();

        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        using DbCommand cmd = conn.CreateCommand();

        cmd.CommandText = $"SELECT * FROM aoai.add_event(@OwnerId, @EventCode, @EventSharedCode, @EventMarkdown, @StartTimestamp, @EndTimestamp, @TimeZoneOffset, @TimeZoneLabel,  @OrganizerName, @OrganizerEmail, @MaxTokenCap, @DailyRequestCap, @Active, @EventImageUrl)";

        cmd.Parameters.Add(new NpgsqlParameter("OwnerId", entraId));
        cmd.Parameters.Add(new NpgsqlParameter("EventCode", newEvent.EventCode));
        cmd.Parameters.Add(new NpgsqlParameter("EventMarkdown", newEvent.EventMarkdown));
        cmd.Parameters.Add(new NpgsqlParameter("StartTimestamp", newEvent.StartTimestamp));
        cmd.Parameters.Add(new NpgsqlParameter("EndTimestamp", newEvent.EndTimestamp));
        cmd.Parameters.Add(new NpgsqlParameter("TimeZoneOffset", newEvent.TimeZoneOffset));
        cmd.Parameters.Add(new NpgsqlParameter("TimeZoneLabel", newEvent.TimeZoneLabel));
        cmd.Parameters.Add(new NpgsqlParameter("OrganizerName", newEvent.OrganizerName));
        cmd.Parameters.Add(new NpgsqlParameter("OrganizerEmail", newEvent.OrganizerEmail));
        cmd.Parameters.Add(new NpgsqlParameter("MaxTokenCap", newEvent.MaxTokenCap));
        cmd.Parameters.Add(new NpgsqlParameter("DailyRequestCap", newEvent.DailyRequestCap));
        cmd.Parameters.Add(new NpgsqlParameter("Active", newEvent.Active));

        var parameter_event_shared_code = new NpgsqlParameter("@EventSharedCode", NpgsqlDbType.Text);
        parameter_event_shared_code.Value = newEvent.EventSharedCode ?? (object)DBNull.Value;
        cmd.Parameters.Add(parameter_event_shared_code);

        var parameter = new NpgsqlParameter("@EventImageUrl", NpgsqlDbType.Text);
        parameter.Value = newEvent.EventImageUrl ?? (object)DBNull.Value;
        cmd.Parameters.Add(parameter);

        var reader = await cmd.ExecuteReaderAsync();

        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
            {
                newEvent.EventId = reader.GetString(0);
            }
        }

        return newEvent;
    }

    public Task<Event?> GetEventAsync(string id) => db.Events.Include(e => e.Catalogs).FirstOrDefaultAsync(e => e.EventId == id);

    public async Task<EventMetric> GetEventMetricsAsync(string eventId)
    {
        (int attendeeCount, int requestCount) = await GetAttendeeMetricsAsync(eventId);
        IEnumerable<(ModelType modelType, string deploymentName, int count)> modelCount = await GetModelCountAsync(eventId);

        return new()
        {
            EventId = eventId,
            AttendeeCount = attendeeCount,
            RequestCount = requestCount,
            ModelCounts = modelCount
        };
    }

    private async Task<IEnumerable<(ModelType modelType, string deploymentName, int count)>> GetModelCountAsync(string eventId)
    {
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        using var modelCountCommand = conn.CreateCommand();
        modelCountCommand.CommandText = """
        SELECT count(api_key) count, resource
        FROM aoai.metric
        WHERE event_id = @EventId
        GROUP BY resource
        ORDER BY count DESC
        """;

        modelCountCommand.Parameters.Add(new NpgsqlParameter("EventId", eventId));
        using var reader = await modelCountCommand.ExecuteReaderAsync();
        List<(ModelType modelType, string deploymentName, int count)> modelCounts = [];
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
            {
                var count = reader.GetInt32(0);
                var resource = reader.GetString(1);

                var parts = resource.Split(" | ");
                var modelType = ModelTypeExtensions.ParsePostgresValue(parts[0]);
                modelCounts.Add((modelType, string.Join(" | ", parts[1..]), count));
            }
        }

        return modelCounts;
    }

    private async Task<(int attendeeCount, int requestCount)> GetAttendeeMetricsAsync(string eventId)
    {
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        using var eventAttendeeCommand = conn.CreateCommand();
        eventAttendeeCommand.CommandText = """
        SELECT
            COUNT(user_id) as user_count,
            (SELECT count(api_key) FROM aoai.metric WHERE event_id = @EventId) as request_count
        FROM aoai.event_attendee
        WHERE event_id = @EventId
        """;

        eventAttendeeCommand.Parameters.Add(new NpgsqlParameter("EventId", eventId));

        using var reader = await eventAttendeeCommand.ExecuteReaderAsync();
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
            {
                return (reader.GetInt32(0), reader.GetInt32(1));
            }
        }

        return (0, 0);
    }

    public async Task<IEnumerable<Event>> GetOwnerEventsAsync()
    {
        string entraId = await authService.GetCurrentUserEntraIdAsync();
        return await db.Events
            .Where(e => e.OwnerEventMaps.Any(o => o.Owner.OwnerId == entraId))
            .OrderByDescending(e => e.Active)
            .ThenBy(e => e.StartTimestamp)
            .ToListAsync();
    }

    public async Task<Event?> UpdateEventAsync(string id, EventEditorModel model)
    {
        Event? evt = await db.Events.FindAsync(id);

        if (evt is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(model.EventSharedCode))
        {
            model.EventSharedCode = null;
        }

        if (string.IsNullOrEmpty(model.EventImageUrl))
        {
            model.EventImageUrl = null;
        }

        evt.EventCode = model.Name!;
        evt.EventSharedCode = model.EventSharedCode;
        evt.EventMarkdown = model.Description!;
        evt.StartTimestamp = model.Start!.Value;
        evt.EndTimestamp = model.End!.Value;
        evt.EventImageUrl = model.EventImageUrl!;
        evt.OrganizerEmail = model.OrganizerEmail!;
        evt.OrganizerName = model.OrganizerName!;
        evt.Active = model.Active;
        evt.MaxTokenCap = model.MaxTokenCap;
        evt.DailyRequestCap = model.DailyRequestCap;
        evt.TimeZoneLabel = model.SelectedTimeZone!.Id;
        evt.TimeZoneOffset = (int)model.SelectedTimeZone.BaseUtcOffset.TotalMinutes;

        await db.SaveChangesAsync();

        return evt;
    }

    public async Task<Event?> UpdateModelsForEventAsync(string id, IEnumerable<Guid> modelIds)
    {
        Event? evt = await db.Events.Include(e => e.Catalogs).FirstOrDefaultAsync(e => e.EventId == id);

        if (evt is null)
        {
            return null;
        }

        evt.Catalogs.Clear();

        IEnumerable<OwnerCatalog> catalogs = await db.OwnerCatalogs.Where(oc => modelIds.Contains(oc.CatalogId)).ToListAsync();

        foreach (OwnerCatalog catalog in catalogs)
        {
            evt.Catalogs.Add(catalog);
        }

        await db.SaveChangesAsync();
        return evt;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            conn.Dispose();
        }
    }
}
