''' Example of using the OpenAI Proxy Python SDK '''

import json
import openai.error
import openai_proxy

openai_proxy.api_key = "hackathon/githubuser"
openai_proxy.api_base = "https://YOUR_OPENAI_PROXY_ENDPOINT"
openai_proxy.api_version = "2023-07-01-preview"
openai_proxy.api_type = "azure"

messages = [
    {
        "role": "system",
        "content": "Don't make assumptions about what values to plug into functions. Ask for clarification if a user request is ambiguous.",
    },
    {"role": "user", "content": "What's the weather like today in seattle"},
]

poem_messages = [
    {
        "role": "system",
        "content": "You are an AI assistant that writes poems in the style of William Shakespeare.",
    },
    {"role": "user", "content": "Write a poem about indian elephants"},
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

try:
    response = openai_proxy.ChatCompletion.create(
        messages=poem_messages,
        max_tokens=256,
        temperature=1.0,
    )

    print(json.dumps(response, indent=4, sort_keys=True))

    response = openai_proxy.ChatCompletion.create(
        messages=messages,
        max_tokens=256,
        temperature=1.0,
        functions=functions,
        function_call={"name": "get_current_weather"},
    )

    print(json.dumps(response, indent=4, sort_keys=True))

    # print the extracted function calls
    for choice in response["choices"]:
        print(choice["message"]["function_call"])

    # this will cause an exception as the function name is invalid
    response = openai_proxy.ChatCompletion.create(
        messages=messages,
        max_tokens=256,
        temperature=1.0,
        functions=functions,
        function_call={"name": "invalid_function_name"},
    )

    print(json.dumps(response, indent=4, sort_keys=True))

except openai.error.InvalidRequestError as invalid_request_error:
    print(invalid_request_error)

except openai.error.AuthenticationError as authentication_error:
    print(authentication_error)

except openai.error.PermissionError as permission_error:
    print(permission_error)

except openai.error.TryAgain as try_again:
    print(try_again)

except openai.error.RateLimitError as rate_limit_error:
    print(rate_limit_error)

except openai.error.APIError as api_error:
    print(api_error)

except Exception as exception:
    print(exception)
