using AzureAIProxy.Models;
using static AzureAIProxy.Routes.Event;

namespace AzureAIProxy.Tests;

public class EventRoutesTests
{
    [Fact]
    public async Task EventInfoReturnsCapabilities()
    {
        HttpContext httpContext = Substitute.For<HttpContext>();
        RequestContext requestContext = new()
        {
            EventId = "event-id",
            IsAuthorized = true,
            MaxTokenCap = 100,
            EventCode = "event-code",
            EventImageUrl = "event-image-url",
            OrganizerName = "organizer-name",
            OrganizerEmail = "organizer-email"
        };
        httpContext.Items["RequestContext"] = requestContext;

        ICatalogService catalogService = Substitute.For<ICatalogService>();

        Dictionary<string, List<string>> capabilities = [];
        capabilities.Add("capability", ["value"]);

        catalogService.GetCapabilities(requestContext.EventId).Returns(capabilities);

        IResult result = await EventInfoAsync(catalogService, httpContext);

        Assert.IsType<Ok<EventInfoResponse>>(result);

        Ok<EventInfoResponse> ok = (Ok<EventInfoResponse>)result;
        Assert.NotNull(ok.Value);
        Assert.Equal(requestContext.IsAuthorized, ok.Value.IsAuthorized);
        Assert.Equal(requestContext.MaxTokenCap, ok.Value.MaxTokenCap);
        Assert.Equal(requestContext.EventCode, ok.Value.EventCode);
        Assert.Equal(requestContext.EventImageUrl, ok.Value.EventImageUrl);
        Assert.Equal(requestContext.OrganizerName, ok.Value.OrganizerName);
        Assert.Equal(requestContext.OrganizerEmail, ok.Value.OrganizerEmail);
        Assert.Equal(capabilities, ok.Value.Capabilities);
    }

    [Fact]
    public async Task InvalidEventIdReturnsNotFound()
    {
        HttpContext httpContext = Substitute.For<HttpContext>();

        IEventService eventService = Substitute.For<IEventService>();

        eventService.GetEventRegistrationInfoAsync("invalid-event-id").Returns((EventRegistration?)null);

        IResult result = await EventRegistrationInfoAsync(eventService, httpContext, "invalid-event-id");

        Assert.IsType<NotFound<string>>(result);

        NotFound<string> notFound = (NotFound<string>)result;
        Assert.Equal("Event not found.", notFound.Value);
    }

    [Fact]
    public async Task ValidEventIdReturnsEventRegistrationInfo()
    {
        HttpContext httpContext = Substitute.For<HttpContext>();

        IEventService eventService = Substitute.For<IEventService>();

        EventRegistration eventRegistrationInfo = new()
        {
            EventId = "event-id",
            EventCode = "event-name",
            EventImageUrl = "event-image-url",
            OrganizerName = "organizer-name",
            OrganizerEmail = "organizer-email",
            EventMarkdown = "event-markdown",
            StartTimestamp = DateTime.UtcNow,
            EndTimestamp = DateTime.UtcNow,
            TimeZoneLabel = "time-zone-label",
            TimeZoneOffset = 0
        };

        eventService.GetEventRegistrationInfoAsync("event-id").Returns(eventRegistrationInfo);

        IResult result = await EventRegistrationInfoAsync(eventService, httpContext, "event-id");

        Assert.IsType<Ok<EventRegistration>>(result);

        Ok<EventRegistration> ok = (Ok<EventRegistration>)result;
        Assert.NotNull(ok.Value);
        Assert.Equal(eventRegistrationInfo.EventId, ok.Value.EventId);
        Assert.Equal(eventRegistrationInfo.EventCode, ok.Value.EventCode);
        Assert.Equal(eventRegistrationInfo.EventImageUrl, ok.Value.EventImageUrl);
        Assert.Equal(eventRegistrationInfo.OrganizerName, ok.Value.OrganizerName);
        Assert.Equal(eventRegistrationInfo.OrganizerEmail, ok.Value.OrganizerEmail);
        Assert.Equal(eventRegistrationInfo.EventMarkdown, ok.Value.EventMarkdown);
        Assert.Equal(eventRegistrationInfo.StartTimestamp, ok.Value.StartTimestamp);
        Assert.Equal(eventRegistrationInfo.EndTimestamp, ok.Value.EndTimestamp);
        Assert.Equal(eventRegistrationInfo.TimeZoneLabel, ok.Value.TimeZoneLabel);
        Assert.Equal(eventRegistrationInfo.TimeZoneOffset, ok.Value.TimeZoneOffset);
    }
}
