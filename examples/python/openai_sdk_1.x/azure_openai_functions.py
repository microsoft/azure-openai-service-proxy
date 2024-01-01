""" Test Azure OpenAI Functions API """

# See documentation at https://gloveboxes.github.io/azure-openai-service-proxy/category/developer-endpoints/

import os

from dotenv import load_dotenv
from openai import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
API_VERSION = "2023-09-01-preview"
DEPLOYMENT_NAME = "gpt-35-turbo"

messages = [
    {
        "role": "system",
        "content": (
            "Don't make assumptions about what values to plug into functions. "
            "Ask for clarification if a user request is ambiguous."
        ),
    },
    {"role": "user", "content": "What's the weather like today in seattle"},
]

functions = [
    {
        "name": "get_current_weather",
        "description": "Get the current weather",
        "parameters": {
            "type": "object",
            "properties": {
                "location": {
                    "type": "string",
                    "description": "The city and state, e.g. San Francisco, CA",
                },
                "format": {
                    "type": "string",
                    "enum": ["celsius", "fahrenheit"],
                    "description": "The temperature unit to use. Infer this from the users location.",
                },
            },
            "required": ["location", "format"],
        },
    },
    {
        "name": "get_n_day_weather_forecast",
        "description": "Get an N-day weather forecast",
        "parameters": {
            "type": "object",
            "properties": {
                "location": {
                    "type": "string",
                    "description": "The city and state, e.g. San Francisco, CA",
                },
                "format": {
                    "type": "string",
                    "enum": ["celsius", "fahrenheit"],
                    "description": "The temperature unit to use. Infer this from the users location.",
                },
                "num_days": {
                    "type": "integer",
                    "description": "The number of days to forecast",
                },
            },
            "required": ["location", "format", "num_days"],
        },
    },
]


client = AzureOpenAI(
    azure_endpoint=ENDPOINT_URL,
    api_key=API_KEY,
    api_version=API_VERSION,
)

completion = client.chat.completions.create(
    model=DEPLOYMENT_NAME,
    messages=messages,
    functions=functions,
)

print(completion.model_dump_json(indent=2))
print()
print(completion.choices[0].finish_reason)
print(completion.choices[0].message.function_call)
