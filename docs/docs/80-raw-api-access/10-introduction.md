# Endpoint access

The Azure OpenAI proxy service provides access to the Azure OpenAI APIs for developers to build applications, again using a time bound event code. Initially, there are three REST endpoints available via the proxy service, `chat completions`, `completions`, and `embeddings`.

## Access information

The event administrator will provide the:

1. `PROXY_ENDPOINT_URL` - The URL of the OpenAI proxy service, eg `https://YOUR_OPENAI_PROXY_ENDPOINT/v1`. The event administrator will provide the URL, note, the `/v1` appended to the URL.
2. The `EVENT_TOKEN` is made up of two parts, the event code followed by your GitHub User Name, eg `hackathon/gloveboxes` and is used where you'd set the OpenAI API key. The event code grants timebound access to the OpenAI APIs and models. The event code is typically the name of the event, eg `hackathon`. The event administrator will provide the event code.

## Proxy service rate limits

The proxy service access is rate limited to balance access to the raw REST APIs to allow fair access for all users.

The proxy service is implemented using FastAPI. FastAPI scales using worker processes, and there can be multiple container replicas of the service. The rate limiter is per worker process and for simplicity, there is no central rate limit tracking, so the rate limiter is per instance of a worker process across each container replica.

The proxy service implements a simple rate limiter for the access to the raw REST APIs. Developers can call the REST APIs up to 200 times in a 10 second period per worker process. If the rate limit is exceeded, the proxy service will return a 429 response code and access to the raw REST API will be blocked for 10 seconds.

The rate limit is intended to only provide a basic level of protection and is not intended to be a full rate limiting solution. If you need a more robust rate limiting solution, then you should consider implementing a rate limiting solution in front of the proxy service.
