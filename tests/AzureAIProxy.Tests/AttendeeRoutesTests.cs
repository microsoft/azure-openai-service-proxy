using static AzureAIProxy.Routes.Attendee;

namespace AzureAIProxy.Tests;

public class AttendeeRoutesTests
{
    [Fact]
    public async Task AddAttendeeCreatedNewApiKey()
    {
        const string attendeeMockKey = "api-key";
        const string userId = "user-id";
        const string eventId = "event-id";

        HttpContext httpContext = Substitute.For<HttpContext>();
        httpContext.Items["RequestContext"] = userId;
        IAttendeeService attendeeService = Substitute.For<IAttendeeService>();
        attendeeService.AddAttendeeAsync(userId, eventId).Returns(attendeeMockKey);

        IResult result = await AddAttendee(attendeeService, httpContext, eventId);

        Assert.IsType<CreatedAtRoute<AttendeeAdded>>(result);

        CreatedAtRoute<AttendeeAdded> created = (CreatedAtRoute<AttendeeAdded>)result;

        Assert.Equal(nameof(GetAttendeeKey), created.RouteName);
        Assert.NotNull(created.Value);
        Assert.Equal(attendeeMockKey, created.Value.ApiKey);

        Received.InOrder(() =>
        {
            attendeeService.AddAttendeeAsync(userId, eventId);
        });
    }

    [Fact]
    public async Task NoAttendeeReturnsNotFound()
    {
        const string userId = "user-id";
        const string eventId = "event-id";

        HttpContext httpContext = Substitute.For<HttpContext>();
        httpContext.Items["RequestContext"] = userId;
        IAttendeeService attendeeService = Substitute.For<IAttendeeService>();
        attendeeService.GetAttendeeKeyAsync(userId, eventId).Returns((AttendeeKey?)null);

        IResult result = await GetAttendeeKey(attendeeService, httpContext, eventId);

        Assert.IsType<NotFound<string>>(result);

        Received.InOrder(() =>
        {
            attendeeService.GetAttendeeKeyAsync(userId, eventId);
        });
    }

    [Fact]
    public async Task AttendeeReturnsOk()
    {
        const string userId = "user-id";
        const string eventId = "event-id";
        const string apiKey = "api-key";

        HttpContext httpContext = Substitute.For<HttpContext>();
        httpContext.Items["RequestContext"] = userId;
        IAttendeeService attendeeService = Substitute.For<IAttendeeService>();
        attendeeService.GetAttendeeKeyAsync(userId, eventId).Returns(new AttendeeKey(apiKey, true));

        IResult result = await GetAttendeeKey(attendeeService, httpContext, eventId);

        Assert.IsType<Ok<AttendeeStatus>>(result);

        Ok<AttendeeStatus> ok = (Ok<AttendeeStatus>)result;

        Assert.NotNull(ok.Value);
        Assert.Equal(apiKey, ok.Value.ApiKey);
        Assert.True(ok.Value.Active);
    }
}
