namespace Proxy.NET;
public class RateLimiteExceededException(string message) : Exception(message)
{
}
