""" Test Azure OpenAI Chat Completions Stream API """

import os
import sys
import time

from dotenv import load_dotenv
from openai import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
API_VERSION = "2023-09-01-preview"
MODEL_NAME = "gpt-35-turbo"


client = AzureOpenAI(
    base_url=ENDPOINT_URL,
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


try:
    response = client.chat.completions.create(
        model=MODEL_NAME,
        messages=[
            {"role": "system", "content": "You are a helpful assistant."},
            {"role": "user", "content": "What is the meaning of life!"},
        ],
        stream=True,
        max_tokens=100,
    )
except Exception as exp:
    print(exp.response.text)
    sys.exit()


for chunk in response:
    if chunk.choices and len(chunk.choices) > 0:
        content = chunk.choices[0].delta.content
        if content:
            print(content, end="", flush=True)
        # delay to simulate real-time chat
        time.sleep(0.05)

print()
