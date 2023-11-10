# Endpoint access

The Azure OpenAI proxy service provides access to the Azure OpenAI APIs for developers to build applications, again using a time bound event code. Initially, there are two REST endpoints available via the proxy service, `chat completions`, and `embeddings`.

## Authentication

The authorization token is made up of the event code and a user id. The user id can be any string, it's recommended to use a GitHub user name. For eample, `hackathon/githubuser`.

The authorization token is passed in the `openai-event-code` header of the REST Post request.
