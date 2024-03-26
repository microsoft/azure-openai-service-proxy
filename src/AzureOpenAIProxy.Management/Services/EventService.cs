using System.Data.Common;
using AzureOpenAIProxy.Management.Components.EventManagement;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.Internal.Postgres;
using NpgsqlTypes;

namespace AzureOpenAIProxy.Management.Services;

public class EventService(IAuthService authService, AoaiProxyContext db) : IEventService
{
    public async Task<Event?> CreateEventAsync(EventEditorModel model)
    {
        Event newEvent = new()
        {
            EventCode = model.Name!,
            EventUrlText = model.UrlText!,
            EventUrl = model.Url!,
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

        using DbConnection conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        using DbCommand cmd = conn.CreateCommand();

        cmd.CommandText = $"SELECT * FROM aoai.add_event(@OwnerId, @EventCode, @EventMarkdown, @StartTimestamp, @EndTimestamp, @TimeZoneOffset, @TimeZoneLabel,  @OrganizerName, @OrganizerEmail, @EventUrl, @EventUrlText, @MaxTokenCap, @DailyRequestCap, @Active, @EventImageUrl)";

        cmd.Parameters.Add(new NpgsqlParameter("OwnerId", entraId));
        cmd.Parameters.Add(new NpgsqlParameter("EventCode", newEvent.EventCode));
        cmd.Parameters.Add(new NpgsqlParameter("EventMarkdown", newEvent.EventMarkdown));
        cmd.Parameters.Add(new NpgsqlParameter("StartTimestamp", newEvent.StartTimestamp));
        cmd.Parameters.Add(new NpgsqlParameter("EndTimestamp", newEvent.EndTimestamp));
        cmd.Parameters.Add(new NpgsqlParameter("TimeZoneOffset", newEvent.TimeZoneOffset));
        cmd.Parameters.Add(new NpgsqlParameter("TimeZoneLabel", newEvent.TimeZoneLabel));
        cmd.Parameters.Add(new NpgsqlParameter("OrganizerName", newEvent.OrganizerName));
        cmd.Parameters.Add(new NpgsqlParameter("OrganizerEmail", newEvent.OrganizerEmail));
        cmd.Parameters.Add(new NpgsqlParameter("EventUrl", newEvent.EventUrl));
        cmd.Parameters.Add(new NpgsqlParameter("EventUrlText", newEvent.EventUrlText));
        cmd.Parameters.Add(new NpgsqlParameter("MaxTokenCap", newEvent.MaxTokenCap));
        cmd.Parameters.Add(new NpgsqlParameter("DailyRequestCap", newEvent.DailyRequestCap));
        cmd.Parameters.Add(new NpgsqlParameter("Active", newEvent.Active));

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
        using DbConnection conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        using DbCommand eventAttendeeCommand = conn.CreateCommand();

        eventAttendeeCommand.CommandText = """
        SELECT
            count(user_id) as user_count,
            (SELECT count(api_key) FROM aoai.metric WHERE event_id = @EventId) as request_count
        FROM aoai.event_attendee
        FROM event_id = @EventId
        """;

        eventAttendeeCommand.Parameters.Add(new NpgsqlParameter("EventId", eventId));

        var reader = await eventAttendeeCommand.ExecuteReaderAsync();

        var metric = new EventMetric() { EventId = eventId };

        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
            {
                metric.AttendeeCount = reader.GetInt32(0);
                metric.RequestCount = reader.GetInt32(1);
            }
        }

        using DbCommand modelCountCommand = conn.CreateCommand();
        modelCountCommand.CommandText = """
        SELECT count(api_key), resource
        FROM aoai.metric
        WHERE event_id = @EventId
        GROUP BY resource
        """;

        modelCountCommand.Parameters.Add(new NpgsqlParameter("EventId", eventId));
        reader = await modelCountCommand.ExecuteReaderAsync();

        if (reader.HasRows)
        {
            List<(ModelType modelType, int count)> modelCounts = [];
            while (await reader.ReadAsync())
            {
                var count = reader.GetInt32(0);
                var resource = reader.GetString(1);

                var parts = resource.Split(" | ");
                var modelType = ModelTypeExtensions.ParsePostgresValue(parts[0]);
                modelCounts.Add((modelType, count));
            }
            metric.ModelCounts = modelCounts;
        }

        return metric;
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

        evt.EventCode = model.Name!;
        evt.EventMarkdown = model.Description!;
        evt.StartTimestamp = model.Start!.Value;
        evt.EndTimestamp = model.End!.Value;
        evt.EventUrl = model.Url!;
        evt.EventUrlText = model.UrlText!;
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
}
