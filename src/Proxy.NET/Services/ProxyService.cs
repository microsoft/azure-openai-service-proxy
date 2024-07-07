using System.Net;
using System.Text;
using System.Text.Json;
using Proxy.NET.Models;

namespace Proxy.NET.Services;

public class ProxyService(IHttpClientFactory httpClientFactory, IMetricService metricService)
    : IProxyService
{
    private const int HttpTimeoutSeconds = 60;

    /// <summary>
    /// Sends an HTTP POST request with the specified JSON object to the specified request URL using the provided endpoint key.
    /// </summary>
    /// <param name="requestJson">The JSON object to send in the request body.</param>
    /// <param name="requestUrl">The URL of the request.</param>
    /// <param name="endpointKey">The endpoint key to use for authentication.</param>
    /// <returns>A tuple containing the response content and the status code of the HTTP response.</returns>
    public async Task<(string responseContent, int statusCode)> HttpPostAsync(
        Uri requestUrl,
        string endpointKey,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    )
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        requestMessage.Content = new StringContent(
            requestJsonDoc.RootElement.ToString(),
            Encoding.UTF8,
            "application/json"
        );
        requestMessage.Headers.Add("api-key", endpointKey);

        var response = await httpClient.SendAsync(requestMessage);
        var responseContent = await response.Content.ReadAsStringAsync();
        await metricService.LogApiUsageAsync(requestContext, deployment, responseContent);

        return (responseContent, (int)response.StatusCode);
    }

    /// <summary>
    /// Sends an HTTP POST request with a stream body asynchronously.
    /// </summary>
    /// <param name="requestUrl">The URL of the request.</param>
    /// <param name="endpointKey">The API key for the endpoint.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="requestString">The request body as a string.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HttpPostStreamAsync(
        Uri requestUrl,
        string endpointKey,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    )
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

        using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl))
        {
            requestMessage.Content = new StringContent(
                requestJsonDoc.RootElement.ToString(),
                Encoding.UTF8,
                "application/json"
            );
            requestMessage.Headers.Add("api-key", endpointKey);

            var response = await httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead
            );
            await metricService.LogApiUsageAsync(requestContext, deployment, null);

            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.ContentType = "application/json";

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            await responseStream.CopyToAsync(context.Response.Body);
        }
    }
}
