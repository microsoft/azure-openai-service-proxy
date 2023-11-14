# Chat completions API

The OpenAI proxy service chat completion endpoint is a REST API that generates a response to a user's prompt. Requests are forwarded to the Azure OpenAI service and the response is returned to the caller.

## Using the OpenAI SDK

The event administrator will provide the:

1. `PROXY_ENDPOINT_URL` - The URL of the OpenAI proxy service, eg `https://YOUR_OPENAI_PROXY_ENDPOINT/v1`. The event administrator will provide the URL, note, the `/v1` appended to the URL.
2. The `EVENT_TOKEN` is made up of two parts, the event code followed by your GitHub User Name, eg `hackathon/gloveboxes`. The event code grants timebound access to the OpenAI APIs and models. The event code is typically the name of the event, eg `hackathon`. The event administrator will provide the event code.

The following example is from the `src/examples` folder and demonstrates how to use the OpenAI SDK to access the chat completion API.

```python
""" Test Azure OpenAI Chat Completions API """

import os
from dotenv import load_dotenv
from openai import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("PROXY_ENDPOINT_URL")
API_KEY = os.environ.get("EVENT_TOKEN")
API_VERSION = "2023-09-01-preview"

# gets the API Key from environment variable AZURE_OPENAI_API_KEY
client = AzureOpenAI(
    azure_endpoint=ENDPOINT_URL,
    api_key=API_KEY,
    api_version=API_VERSION,
)

MESSAGES = [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Who won the world series in 2020?"},
    {
        "role": "assistant",
        "content": "The Los Angeles Dodgers won the World Series in 2020.",
    },
    {"role": "user", "content": "Where was it played?"},
]


completion = client.chat.completions.create(
    model="deployment-name",  # e.g. gpt-35-instant
    messages=MESSAGES,
)

print(completion.model_dump_json(indent=2))
print()
print(completion.choices[0].message.content)

```

## Chat completion with Curl

You can also use `curl` to access the chat completion API.

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

You can also call the OpenAI chat completion API using Python and the httpx library.

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
