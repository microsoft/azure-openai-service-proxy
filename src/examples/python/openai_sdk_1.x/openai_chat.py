""" Test script for OpenAI chat API """

import os
from dotenv import load_dotenv
from openai import OpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
MODEL_NAME = "text-davinci-002"

MESSAGES = [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Who won the world series in 2020?"},
    {
        "role": "assistant",
        "content": "The Los Angeles Dodgers won the World Series in 2020.",
    },
    {"role": "user", "content": "Where was it played?"},
]

client = OpenAI(
    base_url=ENDPOINT_URL,
    api_key=API_KEY,
)


completion = client.chat.completions.create(
    model=MODEL_NAME, messages=MESSAGES  # e.g. gpt-35-instant
)

print(completion.model_dump_json(indent=2))
print()
print(completion.choices[0].message.content)
