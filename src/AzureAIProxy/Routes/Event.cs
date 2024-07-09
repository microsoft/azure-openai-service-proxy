using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Models;
using AzureAIProxy.Services;
using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Routes;

public static class Event
{
    public static RouteGroupBuilder MapEventRoutes(this RouteGroupBuilder builder)
    {
        builder.MapPost("/eventinfo", EventInfoAsync);
        builder.MapGet("/event/{eventId}", EventRegistrationInfoAsync);
        return builder;
    }

    [ApiKeyAuthorize]
    private static async Task<IResult> EventInfoAsync(
        [FromServices] ICatalogService catalogService,
        HttpContext context
    )
    {
        RequestContext requestContext = (RequestContext)context.Items["RequestContext"]!;
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

    private static async Task<IResult> EventRegistrationInfoAsync(
        [FromServices] IEventService eventService,
        HttpContext context,
        string eventId
    )
    {
        var eventRegistrationInfo = await eventService.GetEventRegistrationInfoAsync(eventId);

        if (eventRegistrationInfo is null)
            return TypedResults.NotFound("Event not found.");

        return TypedResults.Ok(eventRegistrationInfo);
    }
}
