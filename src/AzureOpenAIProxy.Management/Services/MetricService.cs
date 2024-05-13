using System.Data;
using System.Data.Common;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AzureOpenAIProxy.Management.Services;

public class ModelCounts
{
    public string? Resource { get; set; }
    public int Count { get; set; }
    public long PromptTokens { get; set; }
    public long CompletionTokens { get; set; }
    public long TotalTokens { get; set; }
}

public class MetricsData
{
    public string EventId { get; set; } = null!;
    public DateTime DateStamp { get; set; }
    public string Resource { get; set; } = null!;
    public long PromptTokens { get; set; }
    public long CompletionTokens { get; set; }
    public long TotalTokens { get; set; }
    public long Requests { get; set; }
}

public class ChartData
{
    public DateTime DateStamp { get; set; }
    public long Count { get; set; }
}

public class ModelData
{
    public List<ModelCounts> ModelCounts { get; set; } = [];
    public List<ChartData> ChartData { get; set; } = [];
}

public class AllEvents
{
    public string OrganizerName { get; set; } = null!;
    public string EventName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Registered { get; set; }
    public string EventId { get; set; } = null!;
}


public class MetricService(IDbContextFactory<AoaiProxyContext> dbContextFactory) : IMetricService, IDisposable
{
    AoaiProxyContext db = dbContextFactory.CreateDbContext();
    DbConnection conn = dbContextFactory.CreateDbContext().Database.GetDbConnection();

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
            db.Dispose();
        }
    }

    public async Task<EventMetric> GetEventMetricsAsync(string eventId)
    {
        (int attendeeCount, int requestCount) = await GetAttendeeMetricsAsync(eventId);
        ModelData modeldata = await GetModelCountAsync(eventId);

        return new()
        {
            EventId = eventId,
            AttendeeCount = attendeeCount,
            RequestCount = requestCount,
            ModelData = modeldata
        };
    }

    private async Task<ModelData> GetModelCountAsync(string eventId)
    {
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        using var modelCountCommand = conn.CreateCommand();
        modelCountCommand.CommandText = """
        SELECT event_id, date_stamp, resource, SUM(prompt_tokens) AS prompt_tokens, SUM(completion_tokens) AS completion_tokens, SUM(total_tokens) AS total_tokens, COUNT(*) AS requests
        FROM aoai.metric_view where event_id = @EventId
        GROUP BY date_stamp, event_id, resource
        ORDER BY requests DESC
        """;

        modelCountCommand.Parameters.Add(new NpgsqlParameter("EventId", eventId));
        using var reader = await modelCountCommand.ExecuteReaderAsync();

        var metricsData = new List<MetricsData>();
        while (reader.Read())
        {
            var item = new MetricsData
            {
                EventId = eventId,
                DateStamp = reader.GetDateTime(1),
                Resource = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),
                PromptTokens = reader.IsDBNull(3) ? 0 : reader.GetInt64(3),
                CompletionTokens = reader.IsDBNull(4) ? 0 : reader.GetInt64(4),
                TotalTokens = reader.IsDBNull(5) ? 0 : reader.GetInt64(5),
                Requests = reader.IsDBNull(6) ? 0 : reader.GetInt64(6)
            };

            metricsData.Add(item);
        };

        var summary = metricsData
            .GroupBy(r => new { EventId = r.EventId, Resource = r.Resource })
            .Select(g => new
            {
                g.Key.Resource,
                PromptTokens = g.Sum(x => (long)x.PromptTokens),
                CompletionTokens = g.Sum(x => (long)x.CompletionTokens),
                TotalTokens = g.Sum(x => (long)x.TotalTokens),
                Requests = g.Sum(x => (long)x.Requests)
            })
            .OrderByDescending(x => x.Requests);

        List<ModelCounts> modelCounts = summary.Select(item => new ModelCounts
        {
            Resource = item.Resource,
            Count = (int)item.Requests,
            PromptTokens = item.PromptTokens,
            CompletionTokens = item.CompletionTokens,
            TotalTokens = item.TotalTokens
        }).ToList();

        // Create Line Chart Data X axis is DateStamp and Y axis is Requests
        List<ChartData> chartData = metricsData
            .GroupBy(r => r.DateStamp)
            .Select(g => new ChartData
            {
                DateStamp = g.Key,
                Count = g.Sum(x => x.Requests)
            })
            .OrderBy(x => x.DateStamp)
            .ToList();

        long runningTotal = 0;
        chartData.ForEach(x => runningTotal = x.Count += runningTotal);

        ModelData md = new()
        {
            ModelCounts = modelCounts,
            ChartData = chartData
        };

        return md;
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

    public async Task<List<AllEvents>> GetAllEventsAsync()
    {
        // create an empty list of AllEvents and populate it with dummy data

        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        using var modelCountCommand = conn.CreateCommand();
        modelCountCommand.CommandText = """
        SELECT
            e.event_id,
            e.event_code,
            e.organizer_name,
            e.start_timestamp,
            e.end_timestamp,
            COUNT(a.api_key) AS registration_count
        FROM
            aoai.event AS e
        LEFT JOIN
            aoai.event_attendee AS a ON e.event_id = a.event_id
        GROUP BY
            e.event_id;
        """;

        using var reader = await modelCountCommand.ExecuteReaderAsync();

        var allEvents = new List<AllEvents>();

        while (reader.Read())
        {
            var item = new AllEvents
            {
                EventId = reader.GetString(0),
                EventName = reader.GetString(1),
                OrganizerName = reader.GetString(2),
                StartDate = reader.GetDateTime(3),
                EndDate = reader.GetDateTime(4),
                Registered = reader.GetInt32(5),

            };
            allEvents.Add(item);
        };
        return allEvents;
    }

    public async Task<List<ChartData>> GetActiveRegistrationsAsync(string eventId)
    {
        // call the Postgres view active_attendee_growth_view, read all the rows and return them as a list of tuples

        if (conn.State != ConnectionState.Open)
            conn.Open();

        using var activeRegistrationsCountCommand = conn.CreateCommand();

        activeRegistrationsCountCommand.CommandText = """
        SELECT
            date_stamp, attendees
        FROM
            aoai.active_attendee_growth_view
        WHERE
            event_id = @EventId
        """;

        activeRegistrationsCountCommand.Parameters.Add(new NpgsqlParameter("EventId", eventId));

        using var reader = await activeRegistrationsCountCommand.ExecuteReaderAsync();

        var activeRegistrations = new List<ChartData>();

        while (reader.Read())
        {
            activeRegistrations.Add(new ChartData { DateStamp = reader.GetDateTime(0), Count = reader.GetInt32(1) });
        };

        return activeRegistrations;
    }
}
