"""Test Azure OpenAI GPT 4 Vision API"""

import os

from dotenv import load_dotenv
from openai import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
API_VERSION = "2023-09-01-preview"
MODEL_NAME = "gpt-4"

IMAGE_URL = "https://welovecatsandkittens.com/wp-content/uploads/2017/05/cute.jpg"

client = AzureOpenAI(
    api_key=API_KEY,
    api_version=API_VERSION,
    base_url=f"{ENDPOINT_URL}/openai/deployments/{MODEL_NAME}/extensions",
)

response = client.chat.completions.create(
    model=MODEL_NAME,
    messages=[
        {"role": "system", "content": "You are a helpful assistant."},
        {
            "role": "user",
            "content": [
                {"type": "text", "text": "Describe this picture:"},
                {
                    "type": "image_url",
                    "image_url": {"url": IMAGE_URL},
                },
            ],
        },
    ],
    max_tokens=2000,
)
print(response)
