# Proxy service rate limits

The proxy service access is rate limited to balance access to the raw REST APIs to allow fair access for all users.

The proxy service is implemented using FastAPI. FastAPI scales using worker processes, and there can be multiple container replicas of the service. The rate limiter is per worker process and for simplicity, there is no central rate limit tracking, so the rate limiter is per instance of a worker process times the number of container replica.

The proxy service implements a simple rate limiter for the access to the raw REST APIs. Developers can call the REST APIs up to 200 times in a 10 second period per worker process. If the rate limit is exceeded, the proxy service will return a 429 response code and access to the raw REST API will be blocked for 10 seconds.

The rate limit is intended to only provide a basic level of protection and is not intended to be a full rate limiting solution. If you need a more robust rate limiting solution, then you should consider implementing a rate limiting solution in front of the proxy service.