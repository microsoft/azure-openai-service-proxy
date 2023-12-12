""" Test OpenAI Functions API """

# See documentation at https://gloveboxes.github.io/azure-openai-service-proxy/category/developer-endpoints/

import os

import openai
from dotenv import load_dotenv

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
MODEL_NAME = "gpt-3.5-turbo"

openai.api_key = API_KEY
openai.api_base = ENDPOINT_URL


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


completion = openai.ChatCompletion.create(
    model=MODEL_NAME,
    messages=messages,
    functions=functions,
)

print(completion)
print()
print(completion.choices[0].finish_reason)
print(completion.choices[0].message.function_call)
