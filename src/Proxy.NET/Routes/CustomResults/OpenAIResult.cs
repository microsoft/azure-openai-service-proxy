using System.Net;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Proxy.NET.Routes.CustomResults;

public class OpenAIResult(string value, HttpStatusCode statusCode) : IResult
{
    private readonly JsonHttpResult<OpenAIErrorPayload> innerResult = TypedResults.Json(
        new OpenAIErrorPayload((int)statusCode, value),
        statusCode: (int)statusCode
    );

    public static OpenAIResult BadRequest(string message) =>
        new(message, HttpStatusCode.BadRequest);

    public static OpenAIResult NotFound(string message) => new(message, HttpStatusCode.NotFound);

    public static OpenAIResult NoContent() => new(string.Empty, HttpStatusCode.NoContent);

    public Task ExecuteAsync(HttpContext httpContext)
    {
        if (innerResult.StatusCode is (int)HttpStatusCode.NoContent)
            return Task.CompletedTask;
        return innerResult.ExecuteAsync(httpContext);
    }

    record OpenAIErrorPayload(int Code, string Message);
}
