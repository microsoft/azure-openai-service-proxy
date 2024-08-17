using System.Text.Json;
using AzureAIProxy.Models;
using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

public interface IProxyService
{
    Task<(string responseContent, int statusCode)> HttpPostAsync(
        UriBuilder requestUrl,
        AuthHeader authHeader,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    );
    Task HttpPostStreamAsync(
        UriBuilder requestUrl,
        AuthHeader authHeader,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    );
    Task<(string responseContent, int statusCode)> HttpGetAsync(
        UriBuilder requestUrl,
        AuthHeader authHeader,
        HttpContext context,
        RequestContext requestContext,
        Deployment deployment
    );
    Task<(string responseContent, int statusCode)> HttpDeleteAsync(
        UriBuilder requestUrl,
        AuthHeader authHeader,
        HttpContext context,
        RequestContext requestContext,
        Deployment deployment
    );
      Task<(string responseContent, int statusCode)> HttpPostFormAsync(
        UriBuilder requestUrl,
        AuthHeader authHeader,
        HttpContext context,
        HttpRequest request,
        RequestContext requestContext,
        Deployment deployment
    );
}
