using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AzureAIProxy.Middleware;

public class OpenAIErrorResponse(string message, HttpStatusCode statusCode)
{
    private readonly JsonHttpResult<ErrorResponse> innerResult = TypedResults.Json(
        new ErrorResponse(new ErrorDetails(statusCode.ToString(), message, (int)statusCode)),
        statusCode: (int)statusCode
    );

    public static OpenAIErrorResponse BadRequest(string message) =>
        new(message, HttpStatusCode.BadRequest);

    public static OpenAIErrorResponse TooManyRequests(string message) =>
        new(message, HttpStatusCode.TooManyRequests);

    public static OpenAIErrorResponse Unauthorized(string message) =>
        new(message, HttpStatusCode.Unauthorized);

    public async Task WriteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = (int)innerResult.StatusCode!;
        await httpContext.Response.WriteAsJsonAsync(innerResult.Value);
    }

    record ErrorDetails(string Code, string Message, int Status);

    record ErrorResponse(ErrorDetails Error);
}
