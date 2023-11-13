from openai import OpenAI

client = OpenAI(
    base_url="YOUR_PROXY_API_URL",
    api_key="YOUR_EVENT_CODE/GITHUB_USERNAME",
)

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

response = client.chat.completions.create(
    messages=messages,
    max_tokens=256,
    model="gpt-3.5-turbo",
    temperature=1.0,
    functions=functions,
    function_call={"name": "get_current_weather"},
)

print(response)
