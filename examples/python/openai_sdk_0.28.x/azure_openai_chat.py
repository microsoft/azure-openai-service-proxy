""" Test Azure OpenAI Chat Completions API """

# See documentation at https://gloveboxes.github.io/azure-openai-service-proxy/category/developer-endpoints/

import os

import openai
from dotenv import load_dotenv

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
API_VERSION = "2023-09-01-preview"
DEPLOYMENT_NAME = "gpt-3.5-turbo"

openai.api_type = "azure"
openai.api_key = API_KEY
openai.api_base = ENDPOINT_URL
openai.api_version = API_VERSION

MESSAGES = [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Who won the world series in 2020?"},
    {
        "role": "assistant",
        "content": "The Los Angeles Dodgers won the World Series in 2020.",
    },
    {"role": "user", "content": "Where was it played?"},
]


completion = openai.ChatCompletion.create(
    deployment_id=DEPLOYMENT_NAME,
    messages=MESSAGES,
)

print(completion)
print()
print(completion.choices[0].message.content)
