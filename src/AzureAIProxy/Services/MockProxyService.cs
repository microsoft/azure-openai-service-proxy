using System.Net;
using System.Text;
using System.Text.Json;
using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

/// <summary>
/// Represents a mock implementation of the <see cref="IProxyService"/> interface.
/// This class is used for testing purposes and provides mock HTTP POST methods.
/// </summary>
public class MockProxyService(IHttpClientFactory httpClientFactory, IMetricService metricService)
    : IProxyService
{
    private const int HttpTimeoutSeconds = 60;
    private static int RandomDelayInMilliseconds
    {
        get => new Random().Next(300, 1501);
    }

    /// <summary>
    /// Sends an HTTP POST request to the specified URL with the provided JSON payload and headers.
    /// This method is used for testing purposes and does not actually send an HTTP request.
    /// </summary>
    public async Task<(string responseContent, int statusCode)> HttpPostAsync(
        string requestUrl,
        string endpointKey,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    )
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

        var requestUrlWithQuery = AppendQueryParameters(requestUrl, context);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrlWithQuery);
        requestMessage.Content = new StringContent(
            requestJsonDoc.RootElement.ToString(),
            Encoding.UTF8,
            "application/json"
        );
        requestMessage.Headers.Add("api-key", endpointKey);

        // Simulate a delay
        await Task.Delay(RandomDelayInMilliseconds);

        // Return a mock response content
        var responseContent = await GetMockResponseContentAsync(
            deployment.ModelType,
            requestUrl.ToString(),
            false
        );

        await metricService.LogApiUsageAsync(requestContext, deployment, responseContent);

        return (responseContent, (int)HttpStatusCode.OK);
    }

    /// <summary>
    /// Sends an HTTP POST request with a stream of data asynchronously.
    /// This method is used for testing purposes and does not actually send an HTTP request.
    /// </summary>
    public async Task HttpPostStreamAsync(
        string requestUrl,
        string endpointKey,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    )
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

        var requestUrlWithQuery = AppendQueryParameters(requestUrl, context);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrlWithQuery);
        requestMessage.Content = new StringContent(
            requestJsonDoc.RootElement.ToString(),
            Encoding.UTF8,
            "application/json"
        );
        requestMessage.Headers.Add("api-key", endpointKey);

        // Simulate a delay
        await Task.Delay(RandomDelayInMilliseconds);

        // Return a mock response content
        var responseContent = await GetMockResponseContentAsync(
            deployment.ModelType,
            requestUrl.ToString(),
            true
        );

        await metricService.LogApiUsageAsync(requestContext, deployment, responseContent);

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(responseContent);
    }

    public async Task<string> GetMockResponseContentAsync(
        string modelType,
        string requestUrl,
        bool streaming
    )
    {
        string extension = streaming ? ".streaming.txt" : ".txt";
        string filePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "MockResponses",
            $"{modelType}{extension}"
        );

        if (File.Exists(filePath))
        {
            return await File.ReadAllTextAsync(filePath);
        }
        else
        {
            return "{\"message\": \"Upstream proxy streaming call skipped for testing purposes\", \"status\": 200, \"requestUrl\": \""
                + requestUrl
                + "\"}";
        }
    }

    /// <summary>
    /// Appends query parameters from the specified <see cref="HttpContext"/> to the given request URL.
    /// </summary>
    /// <param name="requestUrl">The request URL to append the query parameters to.</param>
    /// <param name="context">The <see cref="HttpContext"/> containing the query parameters.</param>
    /// <returns>A new <see cref="Uri"/> object with the appended query parameters.</returns>
    private static Uri AppendQueryParameters(string requestUrl, HttpContext context)
    {
        var queryParameters = context.Request.Query
            .Select(q => $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(q.Value!)}");
        var uriBuilder = new UriBuilder(requestUrl)
        {
            Query = string.Join("&", queryParameters)
        };
        return uriBuilder.Uri;
    }
}
