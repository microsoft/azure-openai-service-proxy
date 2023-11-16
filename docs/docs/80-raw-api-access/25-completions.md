# Completions API

The OpenAI proxy service completion endpoint is a REST API that generates a response to a prompts. Requests are forwarded to the Azure OpenAI service and the response is returned to the caller.

## Using the OpenAI SDK

The following example is from the `src/examples` folder and demonstrates how to use the OpenAI Python SDK version 1.2.x to access the completions API.

```python
""" Test completions with openai """ ""

import os
from dotenv import load_dotenv
import openai


load_dotenv()

ENDPOINT_URL = os.environ.get("PROXY_ENDPOINT_URL")
API_KEY = os.environ.get("EVENT_TOKEN")
API_VERSION = "2023-09-01-preview"

DEPLOYMENT_NAME = "davinci-002"
ENGINE_NAME = "text-davinci-002-prod"

openai.api_key = API_KEY
openai.api_base = ENDPOINT_URL


response = openai.Completion.create(
    engine=ENGINE_NAME, prompt="This is a test", max_tokens=5
)

print(response)
```

## OpenAI completions with Curl

You can also use `curl` to access the OpenAI completions API. Remember, the `API_KEY` is the EventCode/GitHubUserName, eg `hackathon/gloveboxes`, and the `ENDPOINT_URL` is proxy url provided by the event administrator.

```shell
curl -X POST \
-H "openai-event-code: API_KEY" \
-H "Content-Type: application/json" \
-d '{
    "max_tokens": 256,
    "temperature": 1,
    "prompt": "Write a poem about indian elephants"
}' \
https://ENDPOINT_URL/v1/completions
```

or pretty print the JSON response with `jq`

```shell
curl -X POST \
-H "openai-event-code: API_KEY" \
-H "Content-Type: application/json" \
-d '{
    "max_tokens": 256,
    "temperature": 1,
    "prompt": "Write a poem about indian elephants"
}' \
https://ENDPOINT_URL/v1/completions | jq
```
