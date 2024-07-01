using Proxy.NET.Models;

namespace Proxy.NET.Endpoints;

/// <summary>
/// Represents a class that provides methods for mapping HTTP routes to request handlers.
/// </summary>
public class MapRoute
{
    private const string pathVersion = "/api/v1";
    public delegate Task ProcessRequestDelegateAsync<TRequestType>(HttpContext context, TRequestType requestType, string extPath = "");

    /// <summary>
    /// Registers a POST endpoint with the specified route path and request processing delegate.
    /// </summary>
    /// <typeparam name="TRequestType">The type of the request.</typeparam>
    /// <param name="routes">The endpoint route builder.</param>
    /// <param name="requestType">The request type.</param>
    /// <param name="processRequestDelegateAsync">The delegate for processing the request asynchronously.</param>
    /// <param name="authType">The authentication type.</param>
    /// <param name="path">The route path.</param>
    /// <param name="extPath">The optional extension path.</param>
    public static void Post<TRequestType>(
        IEndpointRouteBuilder routes,
        TRequestType requestType,
        ProcessRequestDelegateAsync<TRequestType> processRequestDelegateAsync,
        Auth.Type authType,
        string path,
        string extPath = ""
    )
    {
        routes
            .MapPost(
                pathVersion + path + extPath,
                async context =>
                {
                    await processRequestDelegateAsync(context, requestType, extPath);
                }
            )
            .WithName(requestType!.ToString()!)
            .WithMetadata(new Auth(authType));
    }

    /// <summary>
    /// Maps a GET endpoint to the specified route path and configures the request processing logic.
    /// </summary>
    /// <typeparam name="TRequestType">The type of the request.</typeparam>
    /// <param name="routes">The endpoint route builder.</param>
    /// <param name="requestType">The request type.</param>
    /// <param name="processRequestDelegateAsync">The delegate that processes the request asynchronously.</param>
    /// <param name="authType">The authentication type.</param>
    /// <param name="path">The route path.</param>
    /// <param name="extPath">The optional extension path.</param>
    public static void Get<TRequestType>(
        IEndpointRouteBuilder routes,
        TRequestType requestType,
        ProcessRequestDelegateAsync<TRequestType> processRequestDelegateAsync,
        Auth.Type authType,
        string path,
        string extPath = ""
    )
    {
        routes
            .MapGet(
                pathVersion + path + extPath,
                async context =>
                {
                    await processRequestDelegateAsync(context, requestType, extPath);
                }
            )
            .WithName(requestType!.ToString()!)
            .WithMetadata(new Auth(authType));
    }
}
