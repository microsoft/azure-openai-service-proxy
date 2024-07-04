namespace Proxy.NET.Routes;

// The ProxyResult class is a custom implementation of the IResult interface that is used to return
// the response content and status code from a proxy service. The ProxyResults passes the response and the status code
// received from the proxy service to the HTTP context.

public class ProxyResult(string responseContent, int statusCode) : IResult
{
    public string ResponseContent { get; } = responseContent;
    public int StatusCode { get; } = statusCode;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        if (ResponseContent != null)
        {
            httpContext.Response.StatusCode = StatusCode;
            httpContext.Response.ContentType = "application/json";
            return httpContext.Response.WriteAsync(ResponseContent);
        }
        else
        {
            return Task.CompletedTask;
        }
    }
}
