# Embeddings API

The OpenAI proxy service also supports the OpenAI Embeddings API. The Embeddings API is a REST API that generates embeddings for a given text. Requests are forwarded to the Azure OpenAI service and the response is returned to the caller.

## Using the OpenAI SDK

The following example is from the `src/examples` folder and demonstrates how to use the OpenAI Python SDK version 1.2.x to access the embeddings API.

```python
""" Test Azure OpenAI Embeddings API """

import os
from dotenv import load_dotenv
from openai import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("PROXY_ENDPOINT_URL")
API_KEY = os.environ.get("EVENT_TOKEN")
API_VERSION = "2023-09-01-preview"
OPENAI_EMBEDDING_ENGINE = "text-embedding-ada-002"


client = AzureOpenAI(
    azure_endpoint=ENDPOINT_URL,
    api_key=API_KEY,
    api_version=API_VERSION,
)


content = (
    "This stunning leather wrap bracelet will add a touch of bohemian flair to your outfit."
    "The bracelet features a braided leather band in a rich brown color, adorned with turquoise beads and silver charms. "
    "The bracelet wraps around your wrist multiple times, creating a layered look that is eye-catching and stylish. "
    "The bracelet is adjustable and has a button closure for a secure fit. "
    "This leather wrap bracelet is the perfect accessory for any occasion, "
    "whether you want to dress up a casual outfit or add some color to a formal one."
)


query_embeddings = client.embeddings.create(
    model=OPENAI_EMBEDDING_ENGINE, input=str(content), encoding_format="float"
)

# print(query_embeddings.model_dump_json(indent=2))
print(query_embeddings.data[0].embedding)
```

## OpenAI Embeddings with Curl

You can also use `curl` to access the OpenAI embeddings API. Remember, the `API_KEY` is the EventCode/GitHubUserName, eg `hackathon/gloveboxes`, and the `ENDPOINT_URL` is proxy url provided by the event administrator.

```shell
curl https://ENDPOINT_URL/v1/embeddings \
  -H "Content-Type: application/json" \
  -H "openai-event-code: API_KEY" \
  -d '{
    "input": "Your text string goes here"
  }'
```

or pretty print the JSON response with `jq`

```shell
curl https://ENDPOINT_URL/v1/embeddings \
  -H "Content-Type: application/json" \
  -H "openai-event-code: API_KEY" \
  -d '{
    "input": "Your text string goes here"
  }' | jq
```
