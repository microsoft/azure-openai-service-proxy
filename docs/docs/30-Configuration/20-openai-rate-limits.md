# Rate limits

Azure OpenAI model deployments have two limits, the first being tokens per minute, and the second being requests per minute. You are most likely to hit the Tokens per minute limit especially as you scale up the number of users using the system.

Tokens per minute is the total number of tokens you can generate per minute. The number of tokens per call to OpenAI Chat API is the sum of the Max Token parameter, plus the tokens that make up your msg (system, assistant, and user), plus best_of parameter setting.

For example, you have a model deployment rated at 500K tokens per minute.

![Rate limits](../media/rate_limits.png)

If users set the Max Token parameter to 2048, with a message of 200 tokens, and you have 100 concurrent users sending on average 6 messages per minute then the total number of tokens per minute would be (2048 + 200) * 6 * 100) = 1348800 tokens per minute. This is well over the 500K tokens per minute limit of the Azure OpenAI model deployment and the system would be rate-limited and the user experience would be poor.

## Max Token Cap

This is where the MaxTokenCap is useful for an event. The MaxTokenCap is the maximum number of tokens per request. This overrides the user's Max Token request for load balancing. For example, if you set the MaxTokenCap to 512, then the total number of tokens per minute would be (512 + 200) * 6 * 100) = 427200 tokens per minute. This is well under the 500K tokens per minute limit of the Azure OpenAI model deployment and will result in a better experience for everyone as it minimizes the chance of hitting the rate limit across the system.

MaxTokenCap is set at the event level. See the next section for information about adding events and setting Max Token limits.

## Proxy service rate limits

The proxy service access is rate limited to balance access to the raw REST APIs to allow fair access for all users.

The proxy service is implemented using FastAPI. FastAPI scales using worker processes, and there can be multiple container replicas of the service. The rate limiter is per worker process and for simplicity, there is no central rate limit tracking, so the rate limiter is per instance of a worker process across each container replica.

The proxy service implements a simple rate limiter for the access to the raw REST APIs. Developers can call the REST APIs up to 200 times in a 10 second period per worker process. If the rate limit is exceeded, the proxy service will return a 429 response code and access to the raw REST API will be blocked for 10 seconds.

The rate limit is intended to only provide a basic level of protection and is not intended to be a full rate limiting solution. If you need a more robust rate limiting solution, then you should consider implementing a rate limiting solution in front of the proxy service.
