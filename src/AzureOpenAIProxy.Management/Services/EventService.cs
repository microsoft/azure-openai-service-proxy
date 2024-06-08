using System.Data;
using System.Data.Common;
using AzureOpenAIProxy.Management.Components.EventManagement;
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

        // Need to close else the next event catalog update fails
        conn.Close();

        return newEvent;
    }

    public Task<Event?> GetEventAsync(string id) => db.Events.Include(e => e.Catalogs).FirstOrDefaultAsync(e => e.EventId == id);

    public async Task<IEnumerable<Event>> GetOwnerEventsAsync()
    {
        string entraId = await authService.GetCurrentUserEntraIdAsync();
        return await db.Events
            .Where(e => e.OwnerEventMaps.Any(o => o.Owner.OwnerId == entraId))
            .OrderByDescending(e => e.Active)
            .ThenByDescending(e => e.EndTimestamp)
            .Include(e => e.Catalogs) // Include the Catalogs collection
            .Include(ea => ea.EventAttendees)
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

    public async Task DeleteEventAsync(string id)
    {
        Event? evt = await db.Events.FindAsync(id);

        if (evt is null)
        {
            return;
        }

        // double check there are no event attendees for the event
        var usageInfo = await db.Events.Where(oc => oc.EventId == id)
            .Select(oc => new
            {
                UsedInEvent = oc.EventAttendees.Count != 0
            })
            .FirstAsync();

        // block deletion when it's in use to avoid cascading deletes
        if (usageInfo.UsedInEvent)
        {
            return;
        }

        db.Events.Remove(evt);
        await db.SaveChangesAsync();
    }
}
