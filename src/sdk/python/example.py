import openai_proxy
import openai.error
import json

openai_proxy.api_key = "event_code/github_acct_name"
openai_proxy.api_base = "https://openai-proxy-23uljr-ca.salmonsea-82a61dba.swedencentral.azurecontainerapps.io"
openai_proxy.api_version = "2023-07-01-preview"
openai_proxy.api_type = "azure"

request = {
    "messages": [
        {
            "role": "system",
            "content": "Don't make assumptions about what values to plug into functions. Ask for clarification if a user request is ambiguous.",
        },
        {"role": "user", "content": "What's the weather like today in seattle"},
    ],
    "max_tokens": 4000,
    "temperature": 1,
    "top_p": 0,
    "stop_sequence": "string",
    "frequency_penalty": 0,
    "presence_penalty": 0,
    "function_call": {"name": "get_current_weather"},
    "functions": [
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
    ],
}

try:
    response = openai_proxy.ChatCompletion.create(request)

    print(json.dumps(response, indent=4, sort_keys=True))

except openai.error.InvalidRequestError as invalid_request_error:
    print(invalid_request_error._message)

except openai.error.AuthenticationError as authentication_error:
    print(authentication_error._message)

except openai.error.PermissionError as permission_error:
    print(permission_error._message)

except openai.error.TryAgain as try_again:
    print(try_again._message)

except openai.error.RateLimitError as rate_limit_error:
    print(rate_limit_error._message)

except openai.error.APIError as api_error:
    print(api_error._message)

except Exception as exception:
    print(exception)
