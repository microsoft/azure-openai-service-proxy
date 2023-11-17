# Image Generation API

The Azure OpenAI proxy service supports the Azure OpenAI Image Generation API, as of November 2023, the OpenAI `Dall-e 2` model is supported. Requests are forwarded to the Azure OpenAI service and the response is returned to the caller.

The is no SDK for the Image Generation API, so, using your preferred programming language, make REST calls to the proxy service Image Generation API endpoint.

## Using cURL to access the Image Generation API

The following example demonstrates how to use `cURL` to access the Image Generation API. Remember, the `EVENT_TOKEN` is the EventCode/GitHubUserName, eg `hackathon/gloveboxes`, and the `PROXY_ENDPOINT_URL` is proxy url provided by the event administrator.

```shell
curl -X POST -H "Content-Type: application/json" -H "api-key: API_KEY" -d '{
  "prompt": "cute picture of a cat",
  "size": "1024x1024",
  "n": 2
}' https://PROXY_ENDPOINT_URL/v1/api/images/generations | jq
```
