using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AzureAIProxy.Middleware;

public class OpenAIErrorResponse(string value, HttpStatusCode statusCode)
{
    private readonly JsonHttpResult<OpenAIErrorPayload> innerResult = TypedResults.Json(
        new OpenAIErrorPayload((int)statusCode, value),
        statusCode: (int)statusCode
    );

    public static OpenAIErrorResponse BadRequest(string message) =>
        new(message, HttpStatusCode.BadRequest);

    public static OpenAIErrorResponse TooManyRequests(string message) => new(message, HttpStatusCode.TooManyRequests);

    public static OpenAIErrorResponse Unauthorized(string message) =>
        new(message, HttpStatusCode.Unauthorized);

    public async Task WriteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = (int)innerResult.StatusCode!;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            code = innerResult.StatusCode,
            message = innerResult.Value
        });
    }

    record OpenAIErrorPayload(int Code, string Message);
}
