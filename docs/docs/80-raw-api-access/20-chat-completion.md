# Chat completions API

The OpenAI proxy service chat completion endpoint is a REST API that generates a response to a messages. Requests are forwarded to the Azure OpenAI service and the response is returned to the caller.

## Using the OpenAI SDK

The following example is from the `src/examples` folder and demonstrates how to use the OpenAI Python SDK version 1.2.x to access the chat completions API.

```python
""" Test Azure OpenAI Chat Completions API """

import os
from dotenv import load_dotenv
from openai import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("EVENT_API_KEYTOKEN")
API_VERSION = "2023-09-01-preview"


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

You can also use `curl` to access the chat completion API. Remember, the `API_KEY` is the EventCode/GitHubUserName, eg `hackathon/gloveboxes`, and the `ENDPOINT_URL` is proxy url provided by the event administrator.

```shell
curl -X POST \
-H "openai-event-code: API_KEY" \
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
https://ENDPOINT_URL/v1/chat/completions
```

or pretty print the JSON response with `jq`

```shell
curl -X POST \
-H "openai-event-code: API_KEY" \
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
https://ENDPOINT_URL/v1/chat/completions | jq
```
