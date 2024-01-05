using System.Data;
using System.Data.Common;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AzureOpenAIProxy.Management;

public partial class AoaiProxyContext
{
    public async Task<Event> CreateEventAsync(Event newEvent, string ownerEntraId)
    {
        using DbConnection conn = Database.GetDbConnection();
        await conn.OpenAsync();
        using DbCommand cmd = conn.CreateCommand();

        cmd.CommandText = $"SELECT * FROM aoai.add_event(@EntraId, @EventCode, @EventMarkdown, @StartUtc, @EndUtc, @OrganiserName, @OrganiserEmail, @EventUrl, @EventUrlText, @MaxTokenCap, @SingleCode, @DailyRequestCap, @Active)";

        cmd.Parameters.Add(new NpgsqlParameter("EntraId", ownerEntraId));
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
