using System.Net;
using System.Text.Json;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET.Endpoints;

public static class Event
{
    private enum RequestType
    {
        EventInfo,
        EventRegistrationInfo
    }

    public static void EventEndpoints(this IEndpointRouteBuilder routes)
    {
        MapRoute.Post(routes, RequestType.EventInfo, EventInfoAsync, Auth.Type.ApiKey, "/eventinfo");
        MapRoute.Get(routes, RequestType.EventRegistrationInfo, EventRegistrationInfoAsync, Auth.Type.None, "/event/{event_id}");
    }

    private static async Task EventInfoAsync(HttpContext context, RequestType requestType, string extPath)
    {
        var services = context.RequestServices;
        var catalogService = services.GetRequiredService<ICatalogService>();
        var deploymentName = "event_info";

        if (context.Items["RequestContext"] is not RequestContext requestContext)
            throw new ArgumentException("Request context not found");

        requestContext.DeploymentName = deploymentName;
        var capabilities = await catalogService.GetCapabilities(requestContext.EventId);

        var eventInfo = new EventInfoResponse
        {
            IsAuthorized = requestContext.IsAuthorized,
            MaxTokenCap = requestContext.MaxTokenCap,
            EventCode = requestContext.EventCode,
            EventImageUrl = requestContext.EventImageUrl,
            OrganizerName = requestContext.OrganizerName,
            OrganizerEmail = requestContext.OrganizerEmail,
            Capabilities = capabilities
        };

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(eventInfo));
    }

    private static async Task EventRegistrationInfoAsync(HttpContext context, RequestType requestType, string extPath)
    {
        var services = context.RequestServices;
        var eventService = services.GetRequiredService<IEventService>();
        if (context.GetRouteData().Values["event_id"] is not string eventId || string.IsNullOrEmpty(eventId))
            throw new ArgumentException("Event ID not found");

        var eventRegistrationInfo =
            await eventService.GetEventRegistrationInfoAsync(eventId!)
            ?? throw new HttpRequestException("Event not found", null, HttpStatusCode.NotFound);

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(eventRegistrationInfo));
    }
}
