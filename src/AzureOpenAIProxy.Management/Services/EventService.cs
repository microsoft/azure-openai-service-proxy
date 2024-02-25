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
        conn.Open();
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

    public Event? GetEvent(string id) => db.Events.Include(e => e.Catalogs).FirstOrDefault(e => e.EventId == id);

    public async Task<IEnumerable<Event>> GetOwnerEventsAsync()
    {
        string entraId = await authService.GetCurrentUserEntraIdAsync();
        return db.Events
            .Where(e => e.OwnerEventMaps.Any(o => o.Owner.OwnerId == entraId))
            .OrderByDescending(e => e.Active)
            .ThenBy(e => e.StartTimestamp)
            .ToList();
    }

    public Event? UpdateEvent(string id, EventEditorModel model)
    {
        Event? evt = db.Events.Find(id);

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

        db.SaveChanges();

        return evt;
    }

    public Event? UpdateModelsForEvent(string id, IEnumerable<Guid> modelIds)
    {
        Event? evt = db.Events.Include(e => e.Catalogs).FirstOrDefault(e => e.EventId == id);

        if (evt is null)
        {
            return null;
        }

        evt.Catalogs.Clear();

        IEnumerable<OwnerCatalog> catalogs = db.OwnerCatalogs.Where(oc => modelIds.Contains(oc.CatalogId)).ToList();

        foreach (OwnerCatalog catalog in catalogs)
        {
            evt.Catalogs.Add(catalog);
        }

        db.SaveChanges();
        return evt;
    }
}
