using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AzureAIProxy.Routes.CustomResults;

public class OpenAIResult(string message, HttpStatusCode statusCode) : IResult
{
    private readonly JsonHttpResult<ErrorResponse> innerResult = TypedResults.Json(
        new ErrorResponse(new OpenAIErrorPayload(statusCode.ToString(), message, (int)statusCode)),
        statusCode: (int)statusCode
    );

    public static OpenAIResult BadRequest(string message) =>
        new(message, HttpStatusCode.BadRequest);

    public static OpenAIResult NotFound(string message) =>
        new(message, HttpStatusCode.NotFound);

    public static OpenAIResult NoContent() =>
        new(string.Empty, HttpStatusCode.NoContent);

    public static OpenAIResult MethodNotAllowed(string message) =>
        new(message, HttpStatusCode.MethodNotAllowed);

    public static OpenAIResult ServiceUnavailable(string message) =>
        new(message, HttpStatusCode.ServiceUnavailable);

    public static OpenAIResult Unauthorized(string message) =>
        new(message, HttpStatusCode.Unauthorized);

    public Task ExecuteAsync(HttpContext httpContext)
    {
        if (innerResult.StatusCode == (int)HttpStatusCode.NoContent)
            return Task.CompletedTask;
        return innerResult.ExecuteAsync(httpContext);
    }

    record OpenAIErrorPayload(string Code, string Message, int Status);
    record ErrorResponse(OpenAIErrorPayload Error);
}
