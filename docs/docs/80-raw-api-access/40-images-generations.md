# Image Generation API

The Azure OpenAI proxy service supports the Azure OpenAI Image Generation API, as of November 2023, the OpenAI `Dall-e 2` model is supported. Requests are forwarded to the Azure OpenAI service and the response is returned to the caller.

The is no SDK for the Image Generation API, so, using your preferred programming language, make REST calls to the proxy service Image Generation API endpoint.

The following example is from the `src/examples` folder and demonstrates how to use httpx to access the images generations API.

## Python Example

```python
""" generate images from OpenAI Dall-e model"""

import os
import json
from dotenv import load_dotenv
import httpx

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")


def generate_images(prompt):
    """post the prompt to the OpenAI API and return the response"""
    url = ENDPOINT_URL + "/images/generations"
    headers = {
        "Content-Type": "application/json",
        "api-key": API_KEY,
    }

    data = {
        "prompt": prompt,
        "n": 5,
        "size": "512x512",
    }

    response = httpx.post(url, headers=headers, json=data, timeout=30)
    return response


result = generate_images("cute picture of a cat")
result = result.json()
print(json.dumps(result, indent=4, sort_keys=True))
```

## Using cURL to access the Image Generation API

The following example demonstrates how to use `cURL` to access the Image Generation API. Remember, the `EVENT_TOKEN` is the EventCode/GitHubUserName, eg `hackathon/gloveboxes`, and the `PROXY_ENDPOINT_URL` is proxy url provided by the event administrator.

```shell
curl -X POST -H "Content-Type: application/json" -H "api-key: API_KEY" -d '{
  "prompt": "cute picture of a cat",
  "size": "1024x1024",
  "n": 2
}' https://PROXY_ENDPOINT_URL/api/v1/images/generations | jq
```
