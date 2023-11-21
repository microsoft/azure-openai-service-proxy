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
        "n": 1,
        "size": "1024x1024",
    }

    response = httpx.post(url, headers=headers, json=data, timeout=30)
    return response


result = generate_images("cute picture of a cat")

result = result.json()

print(json.dumps(result, indent=4, sort_keys=True))
