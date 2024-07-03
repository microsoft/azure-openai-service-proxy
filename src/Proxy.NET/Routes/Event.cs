using System.Net;
using Microsoft.AspNetCore.Mvc;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET.Routes;

public static class Event
{
    public static RouteGroupBuilder MapEventRoutes(this RouteGroupBuilder builder)
    {
        builder.MapPost("/eventinfo", EventInfoAsync).WithMetadata(new Auth(Auth.Type.ApiKey));
        builder.MapGet("/event/{eventId}", EventRegistrationInfoAsync).WithMetadata(new Auth(Auth.Type.None));
        return builder;
    }

    /// <summary>
    /// Retrieves event information asynchronously.
    /// </summary>
    /// <param name="catalogService">The catalog service used to retrieve event capabilities.</param>
    /// <param name="context">The HTTP context.</param>
    /// <returns>EventInfoResponse</returns>
    private static async Task<IResult> EventInfoAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IRequestService requestService,
        HttpContext context
    )
    {
        RequestContext requestContext = (RequestContext)requestService.GetRequestContext();
        requestContext.DeploymentName = "event_info";
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

        return TypedResults.Ok(eventInfo);
    }

    /// <summary>
    /// Retrieves the registration information for an event.
    /// </summary>
    /// <param name="eventService">The event service used to retrieve the registration information.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>The task result contains the registration information for the event.</returns>
    private static async Task<IResult> EventRegistrationInfoAsync(
        [FromServices] IEventService eventService,
        HttpContext context,
        string eventId
    )
    {
        var eventRegistrationInfo =
            await eventService.GetEventRegistrationInfoAsync(eventId!)
            ?? throw new HttpRequestException("Event not found", null, HttpStatusCode.NotFound);

        return TypedResults.Ok(eventRegistrationInfo);
    }
}
