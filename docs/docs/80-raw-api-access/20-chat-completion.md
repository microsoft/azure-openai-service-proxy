# Chat completions API

The OpenAI proxy service chat completion endpoint is a REST API that generates a response to a user's prompt. Requests are forwarded to the Azure OpenAI service and the response is returned to the caller.

## Chat completion with Curl

```shell
curl -X POST \
-H "openai-event-code: hackathon/githubuser" \
-H "Content-Type: application/json" \
-d '{
    "max_tokens": 256,
    "temperature": 1,
    "messages": [
        {
            "role": "system",
            "content": "You are an AI assistant that writes poems in the style of William Shakespeare."
        },
        {
            "role": "user",
            "content": "Write a poem about indian elephants"
        }
    ]
}' \
https://YOUR_OPENAI_PROXY_ENDPOINT/api/v1/chat/completions
```

or pretty print the JSON response with `jq`

```shell
curl -X POST \
-H "openai-event-code: hackathon/githubuser" \
-H "Content-Type: application/json" \
-d '{
    "max_tokens": 256,
    "temperature": 1,
    "messages": [
        {
            "role": "system",
            "content": "You are an AI assistant that writes poems in the style of William Shakespeare."
        },
        {
            "role": "user",
            "content": "Write a poem about indian elephants"
        }
    ]
}' \
https://YOUR_OPENAI_PROXY_ENDPOINT/api/v1/chat/completions | jq
```

## Chat completion with Python and httpx

```python

request = {
    "max_tokens": 256,
    "temperature": 1,
    "messages": [
        {
            "role": "system",
            "content": "You are an AI assistant that writes poems in the style of William Shakespeare."
        },
        {
            "role": "user",
            "content": "Write a poem about indian elephants"
        }
    ]
}

url = "https://YOUR_OPENAI_PROXY_ENDPOINT/api/v1/chat/completions"

headers = {"openai-event-code": hackathon/githubuser}

response = httpx.post(url=url, headers=headers, json=request, timeout=30)

if response.status_code == 200:
    print(response.json())
```

## The Python OpenAI Proxy SDK

The Python OpenAI Proxy SDK wraps the REST API calls to the proxy service. The wrapper is designed to be a drop in replacement for the official OpenAI Chat Completion Python API.

```python
''' Example of using the OpenAI Proxy Python SDK '''

import json
import openai.error
import openai_proxy

openai_proxy.api_key = "hackathon/githubuser"
openai_proxy.api_base = "https://YOUR_OPENAI_PROXY_ENDPOINT"
openai_proxy.api_version = "2023-07-01-preview"
openai_proxy.api_type = "azure"

poem_messages = [
    {
        "role": "system",
        "content": "You are an AI assistant that writes poems in the style of William Shakespeare.",
    },
    {"role": "user", "content": "Write a poem about indian elephants"},
]

try:
    response = openai_proxy.ChatCompletion.create(
        messages=poem_messages,
        max_tokens=256,
        temperature=1.0,
    )

    print(json.dumps(response, indent=4, sort_keys=True))

except openai.error.InvalidRequestError as invalid_request_error:
    print(invalid_request_error)

except openai.error.AuthenticationError as authentication_error:
    print(authentication_error)

except openai.error.PermissionError as permission_error:
    print(permission_error)

except openai.error.TryAgain as try_again:
    print(try_again)

except openai.error.RateLimitError as rate_limit_error:
    print(rate_limit_error)

except openai.error.APIError as api_error:
    print(api_error)

except Exception as exception:
    print(exception)
```


A complete working example can be found in the [Python OpenAI Proxy SDK](https://github.com/gloveboxes/azure-openai-service-proxy/tree/main/src/sdk/python) folder.

