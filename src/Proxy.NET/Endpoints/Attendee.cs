using System.Net;
using System.Text.Json;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET.Endpoints;

public static class Attendee
{
    private enum RequestType
    {
        AttendeeAdd,
        AttendeeGetKey
    }

    public static void AttendeeEndpoints(this IEndpointRouteBuilder routes)
    {
        MapRoute.Post(routes, RequestType.AttendeeAdd, AttendeeAdd, Auth.Type.Jwt, "/attendee/event/{event_id}/register");
        MapRoute.Get(routes, RequestType.AttendeeGetKey, AttendeeGetKey, Auth.Type.Jwt, "/attendee/event/{event_id}");
    }

    /// <summary>
    /// Represents an asynchronous operation that can return a value.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task AttendeeAdd(HttpContext context, RequestType requestType, string extPath)
    {
        var attendeeService = context.RequestServices.GetRequiredService<IAttendeeService>();
        if (context.Items["RequestContext"] is not string userId || string.IsNullOrEmpty(userId))
            throw new ArgumentException("Request context not found");
        if (context.GetRouteData().Values["event_id"] is not string eventId || string.IsNullOrEmpty(eventId))
            throw new ArgumentException("Event ID not found");

        string api_key = await attendeeService.AddAttendeeAsync(userId, eventId);
        context.Response.StatusCode = (int)HttpStatusCode.Created;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { api_key }));
    }

    /// <summary>
    /// Asynchronously retrieves the attendee key for a given user and event.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="requestType">The type of the request.</param>
    /// <param name="extPath">The extension path.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task AttendeeGetKey(HttpContext context, RequestType requestType, string extPath)
    {
        var attendeeService = context.RequestServices.GetRequiredService<IAttendeeService>();
        if (context.Items["RequestContext"] is not string userId || string.IsNullOrEmpty(userId))
            throw new ArgumentException("Request context not found");
        if (context.GetRouteData().Values["event_id"] is not string eventId || string.IsNullOrEmpty(eventId))
            throw new ArgumentException("Event ID not found");

        (string apiKey, bool active) = await attendeeService.GetAttendeeKeyAsync(userId, eventId);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { api_key = apiKey, active }));
    }
}
