# Endpoint access

The Azure OpenAI proxy service provides access to the Azure OpenAI APIs for developers to build applications, again using a time bound event code. Initially, the following APIs are available via the proxy service:

1. `chat completions`
2. `completions`,
3. `embeddings`,
4. `images generations`.

## Access information

The event administrator will provide the:

1. `ENDPOINT_URL` - The URL of the OpenAI proxy service, eg `https://YOUR_OPENAI_PROXY_ENDPOINT/api/v1`. The event administrator will provide the URL, note, the `/api/v1` appended to the end of URL.
2. The `API_KEY` is made up of two parts, the event code followed by your GitHub User Name, seperated by a slash, eg `hackathon/gloveboxes`. The event code grants timebound access to the OpenAI APIs and models. The event code is typically the name of the event, eg `hackathon`. The event administrator will provide the event code.

## Examples

There are examples in the `src/examples` folder that demonstrate how to use the proxy service to access the Azure OpenAI APIs.
