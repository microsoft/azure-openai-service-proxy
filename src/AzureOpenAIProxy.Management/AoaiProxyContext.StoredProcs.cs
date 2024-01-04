using System.Data;
using System.Data.Common;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AzureOpenAIProxy.Management;

public partial class AoaiProxyContext
{
    public async Task<Event> CreateEventAsync(Event newEvent, Guid ownerId)
    {
        using DbConnection conn = Database.GetDbConnection();
        await conn.OpenAsync();
        using DbCommand cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.CommandText = $"aoai.add_event p_owner_id=@OwnerId p_event_code=@EventCode p_event_markdown=@EventMarkdown p_start_utc=@StartUtc p_end_utc=@EndUtc p_organizer_name=@OrganiserName p_organizer_email=@OrganiserEmail p_event_url=@EventUrl p_event_url_text=@EventUrlText p_max_token_cap=@MaxTokenCap p_single_code=@SingleCode p_daily_request_cap=@DailyRequestCap p_active=@Active";

        cmd.Parameters.Add(new NpgsqlParameter("OwnerId", ownerId));
        cmd.Parameters.Add(new NpgsqlParameter("EventCode", newEvent.EventCode));
        cmd.Parameters.Add(new NpgsqlParameter("EventMarkdown", newEvent.EventMarkdown));
        cmd.Parameters.Add(new NpgsqlParameter("StartUtc", newEvent.StartUtc));
        cmd.Parameters.Add(new NpgsqlParameter("EndUtc", newEvent.EndUtc));
        cmd.Parameters.Add(new NpgsqlParameter("OrganiserName", newEvent.OrganizerName));
        cmd.Parameters.Add(new NpgsqlParameter("OrganiserEmail", newEvent.OrganizerEmail));
        cmd.Parameters.Add(new NpgsqlParameter("EventUrl", newEvent.EventUrl));
        cmd.Parameters.Add(new NpgsqlParameter("EventUrlText", newEvent.EventUrlText));
        cmd.Parameters.Add(new NpgsqlParameter("MaxTokenCap", newEvent.MaxTokenCap));
        cmd.Parameters.Add(new NpgsqlParameter("SingleCode", newEvent.SingleCode));
        cmd.Parameters.Add(new NpgsqlParameter("DailyRequestCap", newEvent.DailyRequestCap));
        cmd.Parameters.Add(new NpgsqlParameter("Active", newEvent.Active));

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
}
