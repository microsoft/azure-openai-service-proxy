using Microsoft.AspNetCore.Mvc;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET.Endpoints;

public static class Attendee
{
    public static RouteGroupBuilder MapAttendeeRoutes(this RouteGroupBuilder builder)
    {
        var attendeeGroup = builder.MapGroup("/attendee/event/{eventId}");
        attendeeGroup.MapPost("/register", AttendeeAdd).WithMetadata(new Auth(Auth.Type.Jwt));
        attendeeGroup.MapGet("/", AttendeeGetKey).WithMetadata(new Auth(Auth.Type.Jwt));
        return builder;
    }

    /// <summary>
    /// Adds an attendee to the specified event.
    /// </summary>
    /// <param name="attendeeService">The attendee service.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>The task result contains the API key of the added attendee.</returns>
    private static async Task<IResult> AttendeeAdd([FromServices] IAttendeeService attendeeService, HttpContext context, string eventId)
    {
        string? userId = context.Items["RequestContext"] as string;
        string api_key = await attendeeService.AddAttendeeAsync(userId!, eventId);
        return TypedResults.Created(context.Request.Path, new { api_key });
    }

    /// <summary>
    /// Retrieves the attendee key for a specific event.
    /// </summary>
    /// <param name="attendeeService">The attendee service.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>The task result contains the attendee key and its activation status.</returns>
    private static async Task<IResult> AttendeeGetKey([FromServices] IAttendeeService attendeeService, HttpContext context, string eventId)
    {
        string? userId = context.Items["RequestContext"] as string;
        (string apiKey, bool active) = await attendeeService.GetAttendeeKeyAsync(userId!, eventId);
        return TypedResults.Ok(new { api_key = apiKey, active });
    }
}
