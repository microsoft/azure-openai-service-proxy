using System.Data;
using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management.Services;

public class EventMetricsData
{
    public string EventId { get; set; } = null!;
    public DateTime DateStamp { get; set; }
    public string Resource { get; set; } = null!;
    public long PromptTokens { get; set; }
    public long CompletionTokens { get; set; }
    public long TotalTokens { get; set; }
    public long Requests { get; set; }
}

public class EventChartData
{
    public DateTime DateStamp { get; set; }
    public long Count { get; set; }
}


public class MetricService(AoaiProxyContext db) : IMetricService
{
    public async Task<List<EventMetricsData>> GetEventMetricsAsync(string eventId)
    {
        var query = from mv in db.MetricViews
                    where mv.EventId == eventId
                    group mv by new { mv.EventId, mv.DateStamp, mv.Resource } into g
                    orderby g.Count() descending
                    select new
                    {
                        g.Key.EventId,
                        g.Key.DateStamp,
                        g.Key.Resource,
                        PromptTokens = g.Sum(x => x.PromptTokens),
                        CompletionTokens = g.Sum(x => x.CompletionTokens),
                        TotalTokens = g.Sum(x => x.TotalTokens),
                        Requests = g.Count()
                    };

        List<EventMetricsData> metricsData = await query.Select(x => new EventMetricsData
        {
            EventId = x.EventId,
            DateStamp = x.DateStamp,
            Resource = x.Resource,
            PromptTokens = x.PromptTokens,
            CompletionTokens = x.CompletionTokens,
            TotalTokens = x.TotalTokens,
            Requests = x.Requests
        }).ToListAsync();

        return metricsData;
    }

    public (int attendeeCount, int requestCount) GetAttendeeMetricsAsync(string eventId)
    {
        var userCount = db.EventAttendees
            .Where(ea => ea.EventId == eventId)
            .Count();

        var requestCount = db.Metrics
            .Where(m => m.EventId == eventId)
            .Count();

        return (userCount, requestCount);
    }

    public async Task<List<EventRegistrations>> GetAllEventsAsync()
    {
        var query = from e in db.Events
                    join a in db.EventAttendees on e.EventId equals a.EventId into ea
                    from a in ea.DefaultIfEmpty()
                    group a by new { e.EventId, e.EventCode, e.OrganizerName, e.StartTimestamp, e.EndTimestamp } into g
                    select new
                    {
                        g.Key.EventId,
                        g.Key.EventCode,
                        g.Key.OrganizerName,
                        g.Key.StartTimestamp,
                        g.Key.EndTimestamp,
                        RegistrationCount = g.Count(a => a.ApiKey != null)
                    };

        var allEvents = await query.Select(x => new EventRegistrations
        {
            EventId = x.EventId,
            EventName = x.EventCode,
            OrganizerName = x.OrganizerName,
            StartDate = x.StartTimestamp,
            EndDate = x.EndTimestamp,
            Registered = x.RegistrationCount
        }).ToListAsync();

        return allEvents;
    }

    public async Task<List<EventChartData>> GetActiveRegistrationsAsync(string eventId)
    {
        var query = from a in db.ActiveAttendeeGrowthViews
                    where a.EventId == eventId
                    select new { a.DateStamp, a.Attendees };

        List<EventChartData> activeRegistrations = await query.Select(x => new EventChartData
        {
            DateStamp = x.DateStamp,
            Count = (int)x.Attendees
        }).ToListAsync();

        return activeRegistrations;
    }
}
