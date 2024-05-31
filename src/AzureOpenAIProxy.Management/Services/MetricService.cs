using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AzureOpenAIProxy.Management.Services;

public class MetricService(AoaiProxyContext db) : IMetricService, IDisposable
{
    private readonly DbConnection conn = db.Database.GetDbConnection();

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

        List<MetricsData> metricsData = [];
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
            .GroupBy(r => new { r.EventId, r.Resource })
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
        List<ChartData> chartData = [
            .. metricsData
            .GroupBy(r => r.DateStamp)
            .Select(g => new ChartData
            {
                DateStamp = g.Key,
                Count = g.Sum(x => x.Requests)
            })
            .OrderBy(x => x.DateStamp)
        ];

        long runningTotal = chartData.Aggregate(0L, (acc, x) => acc += x.Count);

        return new()
        {
            ModelCounts = modelCounts,
            ChartData = chartData
        };
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

        List<ChartData> activeRegistrations = [];

        while (reader.Read())
        {
            activeRegistrations.Add(new ChartData { DateStamp = reader.GetDateTime(0), Count = reader.GetInt32(1) });
        };

        return activeRegistrations;
    }

    internal class MetricsData
    {
        public string EventId { get; set; } = null!;
        public DateTime DateStamp { get; set; }
        public string Resource { get; set; } = null!;
        public long PromptTokens { get; set; }
        public long CompletionTokens { get; set; }
        public long TotalTokens { get; set; }
        public long Requests { get; set; }
    }
}
