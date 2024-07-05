using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

namespace Proxy.NET.Routes.CustomResults;

public class OpenAIResult(string value, HttpStatusCode statusCode) : IResult
{
    private readonly JsonHttpResult<OpenAIErrorPayload> innerResult =
        TypedResults.Json(new OpenAIErrorPayload((int)statusCode, value), statusCode: (int)statusCode);

    public static OpenAIResult BadRequest(string message) => new(message, HttpStatusCode.BadRequest);

    public static OpenAIResult NotFound(string message) => new(message, HttpStatusCode.NotFound);

    public Task ExecuteAsync(HttpContext httpContext) =>
        innerResult.ExecuteAsync(httpContext);

    record OpenAIErrorPayload(int Code, string Message);
}
