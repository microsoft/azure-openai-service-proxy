using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Services;

namespace AzureAIProxy.Routes;

public static class Attendee
{
    public static RouteGroupBuilder MapAttendeeRoutes(this RouteGroupBuilder builder)
    {
        var attendeeGroup = builder.MapGroup("/attendee/event/{eventId}");
        attendeeGroup.MapPost("/register", AttendeeAdd);
        attendeeGroup.MapGet("/", AttendeeGetKey);
        return builder;
    }

    [JwtAuthorize]
    private static async Task<IResult> AttendeeAdd(
        [FromServices] IAttendeeService attendeeService,
        HttpContext context,
        string eventId
    )
    {
        string userId = (string)context.Items["RequestContext"]!;
        string api_key = await attendeeService.AddAttendeeAsync(userId, eventId);
        return TypedResults.Created(context.Request.Path, new { api_key });
    }

    [JwtAuthorize]
    private static async Task<IResult> AttendeeGetKey(
        [FromServices] IAttendeeService attendeeService,
        HttpContext context,
        string eventId
    )
    {
        string userId = (string)context.Items["RequestContext"]!;
        var attendee = await attendeeService.GetAttendeeKeyAsync(userId, eventId);

        if (attendee is null)
            return TypedResults.NotFound("Attendee not found.");

        return TypedResults.Ok(new { attendee.ApiKey, attendee.Active });
    }
}
