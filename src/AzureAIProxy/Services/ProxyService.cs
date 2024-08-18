using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using AzureAIProxy.Models;
using AzureAIProxy.Shared.Database;
using Microsoft.Extensions.Primitives;

namespace AzureAIProxy.Services;

/// <summary>
/// Provides methods for sending HTTP requests (GET, POST, DELETE) to specified URLs with support for various content types and query parameters.
/// </summary>
public class ProxyService(IHttpClientFactory httpClientFactory, IMetricService metricService)
    : IProxyService
{
    private const int HttpTimeoutSeconds = 60;

    /// <summary>
    /// Sends an HTTP DELETE request to the specified URL using the provided endpoint key.
    /// </summary>
    /// <param name="requestUrl">The URL to which the DELETE request is sent.</param>
    /// <param name="endpointKey">The API key used for authorization.</param>
    /// <param name="context">The HTTP context containing additional information for the request.</param>
    /// <param name="requestContext">The request context object containing relevant details for the request.</param>
    /// <param name="deployment">The deployment details related to the request.</param>
    /// <returns>A tuple containing the response content as a string and the HTTP status code.</returns>
    public async Task<(string responseContent, int statusCode)> HttpDeleteAsync(
        UriBuilder requestUrl,
        List<RequestHeader> requestHeaders,
        HttpContext context,
        RequestContext requestContext,
        Deployment deployment
    )
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

        var requestUrlWithQuery = AppendQueryParameters(requestUrl, context);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUrlWithQuery);

        foreach (var header in requestHeaders)
            requestMessage.Headers.Add(header.Key, header.Value);

        var response = await httpClient.SendAsync(requestMessage);
        var responseContent = await response.Content.ReadAsStringAsync();

        return (responseContent, (int)response.StatusCode);
    }

    /// <summary>
    /// Sends an HTTP GET request to the specified URL using the provided endpoint key.
    /// </summary>
    /// <param name="requestUrl">The URL to which the GET request is sent.</param>
    /// <param name="endpointKey">The API key used for authorization.</param>
    /// <param name="context">The HTTP context containing additional information for the request.</param>
    /// <param name="requestContext">The request context object containing relevant details for the request.</param>
    /// <param name="deployment">The deployment details related to the request.</param>
    /// <returns>A tuple containing the response content as a string and the HTTP status code.</returns>

    public async Task<(string responseContent, int statusCode)> HttpGetAsync(
        UriBuilder requestUrl,
        List<RequestHeader> requestHeaders,
        HttpContext context,
        RequestContext requestContext,
        Deployment deployment
    )
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

        var requestUrlWithQuery = AppendQueryParameters(requestUrl, context);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrlWithQuery);

        foreach (var header in requestHeaders)
            requestMessage.Headers.Add(header.Key, header.Value);

        var response = await httpClient.SendAsync(requestMessage);

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        switch (mediaType)
        {
            case string type when type.Contains("application/json"):
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return (responseContent, (int)response.StatusCode);
                }

            case string type when type.Contains("application/octet-stream"):
                {
                    context.Response.ContentType = "application/octet-stream";
                    await using var responseStream = await response.Content.ReadAsStreamAsync();
                    await responseStream.CopyToAsync(context.Response.Body);
                    return (string.Empty, (int)response.StatusCode);
                }

            default:
                return (string.Empty, (int)HttpStatusCode.UnsupportedMediaType);
        }
    }

    /// <summary>
    /// Sends an HTTP POST request with a form body asynchronously.
    /// </summary>
    /// <param name="requestUrl">The URL of the request.</param>
    /// <param name="endpointKey">The API key for the endpoint.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="request">The HTTP request containing form data.</param>
    /// <param name="requestContext">The request context object containing relevant details for the request.</param>
    /// <param name="deployment">The deployment details related to the request.</param>
    /// <returns>A tuple containing the response content as a string and the HTTP status code.</returns>
    public async Task<(string responseContent, int statusCode)> HttpPostFormAsync(
        UriBuilder requestUrl,
        List<RequestHeader> requestHeaders,
        HttpContext context,
        HttpRequest request,
        RequestContext requestContext,
        Deployment deployment
    )
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

        var form = await request.ReadFormAsync();
        var file = form.Files.GetFile("file");

        if (file is not null && file.Length > 0)
        {
            var requestUrlWithQuery = AppendQueryParameters(requestUrl, context);

            var fileStream = file.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            // Prepare the multipart form content
            var multipartContent = new MultipartFormDataContent
            {
                { fileContent, "file", file.FileName }
            };

            foreach (var key in form.Keys.Where(k => k != "file" && !StringValues.IsNullOrEmpty(form[k])))
            {
                var encodedValue = HttpUtility.HtmlEncode(form[key]!);
                var fieldContent = new StringContent(encodedValue);
                multipartContent.Add(fieldContent, key);
            }

            // Create the request message
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrlWithQuery)
            {
                Content = multipartContent
            };

            foreach (var header in requestHeaders)
                requestMessage.Headers.Add(header.Key, header.Value);

            var response = await httpClient.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();

            await metricService.LogApiUsageAsync(requestContext, deployment, responseContent);

            return (responseContent, (int)response.StatusCode);
        }
        else
        {
            return (string.Empty, (int)HttpStatusCode.UnsupportedMediaType);
        }
    }

    /// <summary>
    /// Sends an HTTP POST request with a JSON body asynchronously.
    /// </summary>
    /// <param name="requestUrl">The URL of the request.</param>
    /// <param name="endpointKey">The API key for the endpoint.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="requestJsonDoc">The JSON document to be sent in the request body.</param>
    /// <param name="requestContext">The request context object containing relevant details for the request.</param>
    /// <param name="deployment">The deployment details related to the request.</param>
    /// <returns>A tuple containing the response content as a string and the HTTP status code.</returns>
    public async Task<(string responseContent, int statusCode)> HttpPostAsync(
        UriBuilder requestUrl,
        List<RequestHeader> requestHeaders,
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

        foreach (var header in requestHeaders)
            requestMessage.Headers.Add(header.Key, header.Value);

        var response = await httpClient.SendAsync(requestMessage);
        var responseContent = await response.Content.ReadAsStringAsync();
        await metricService.LogApiUsageAsync(requestContext, deployment, responseContent);

        return (responseContent, (int)response.StatusCode);
    }

    /// <summary>
    /// Sends an HTTP POST request with a JSON body and streams the response body asynchronously.
    /// </summary>
    /// <param name="requestUrl">The URL of the request.</param>
    /// <param name="endpointKey">The API key for the endpoint.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="requestJsonDoc">The JSON document to be sent in the request body.</param>
    /// <param name="requestContext">The request context object containing relevant details for the request.</param>
    /// <param name="deployment">The deployment details related to the request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HttpPostStreamAsync(
        UriBuilder requestUrl,
        List<RequestHeader> requestHeaders,
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

        foreach (var header in requestHeaders)
            requestMessage.Headers.Add(header.Key, header.Value);

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

    /// <summary>
    /// Appends query parameters from the specified <see cref="HttpContext"/> to the given request URL.
    /// </summary>
    /// <param name="requestUrl">The <see cref="UriBuilder"/> object representing the URL to which the query parameters will be appended.</param>
    /// <param name="context">The <see cref="HttpContext"/> containing the query parameters to be appended to the URL.</param>
    /// <returns>A new <see cref="Uri"/> object with the appended query parameters.</returns>
    /// <remarks>
    /// This method iterates over the query parameters in the <see cref="HttpContext"/>'s request, skips any parameters with empty values,
    /// and appends the remaining parameters to the query string of the provided <see cref="UriBuilder"/>.
    /// The resulting <see cref="Uri"/> object reflects the updated URL with the added query parameters.
    /// </remarks>
    private static Uri AppendQueryParameters(UriBuilder requestUrl, HttpContext context)
    {
        var queryParameters = context.Request.Query
            .Where(q => !string.IsNullOrEmpty(q.Value)) // Skip parameters with empty values
            .Select(q => $"{q.Key}={q.Value!}");

        requestUrl.Query = string.Join("&", queryParameters);
        return requestUrl.Uri;
    }
}
