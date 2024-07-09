using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Services;
using System.Text.Json.Serialization;

namespace AzureAIProxy.Routes;

public static class Attendee
{
    public static RouteGroupBuilder MapAttendeeRoutes(this RouteGroupBuilder builder)
    {
        var attendeeGroup = builder.MapGroup("/attendee/event/{eventId}");
        attendeeGroup.MapPost("/register", AddAttendee);
        attendeeGroup.MapGet("/", GetAttendeeKey).WithName(nameof(GetAttendeeKey));
        return builder;
    }

    [JwtAuthorize]
    internal static async Task<IResult> AddAttendee(
        [FromServices] IAttendeeService attendeeService,
        HttpContext context,
        string eventId
    )
    {
        string userId = (string)context.Items["RequestContext"]!;
        string attendeeApiKey = await attendeeService.AddAttendeeAsync(userId, eventId);
        return TypedResults.CreatedAtRoute(new AttendeeAdded(attendeeApiKey), nameof(GetAttendeeKey));
    }

    [JwtAuthorize]
    internal static async Task<IResult> GetAttendeeKey(
        [FromServices] IAttendeeService attendeeService,
        HttpContext context,
        string eventId
    )
    {
        string userId = (string)context.Items["RequestContext"]!;
        var attendee = await attendeeService.GetAttendeeKeyAsync(userId, eventId);

        if (attendee is null)
            return TypedResults.NotFound("Attendee not found.");

        return TypedResults.Ok(new AttendeeStatus(attendee.ApiKey, attendee.Active));
    }

    internal record AttendeeAdded([property: JsonPropertyName("api_key")] string ApiKey);
    internal record AttendeeStatus(string ApiKey, bool Active);
}
