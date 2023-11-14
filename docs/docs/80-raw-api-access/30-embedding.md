# Embeddings API

The OpenAI proxy service also supports the OpenAI Embeddings API. The Embeddings API is a REST API that generates embeddings for a given text. Requests are forwarded to the Azure OpenAI service and the response is returned to the caller.

## Embeddings with Curl

```shell
curl https://api.openai.com/v1/embeddings \
  -H "Content-Type: application/json" \
  -H "openai-event-code: hackathon/githubuser" \
  -d '{
    "input": "Your text string goes here"
  }'
```

or pretty print the JSON response with `jq`

```shell
curl https://api.openai.com/v1/embeddings \
  -H "Content-Type: application/json" \
  -H "openai-event-code: hackathon/githubuser" \
  -d '{
    "input": "Your text string goes here"
  }' | jq
```

