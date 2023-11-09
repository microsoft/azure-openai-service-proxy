# Intro to OpenAI Functions

This sample uses OpenAI Functions extensive to power the home assistant. OpenAI Functions enables you to describe functions to gpt-3.5-turbo-0613 and gpt-4-0613 models and later, and have the GPT model intelligently select which function (if any) best matches the data in the prompt. The function definitions along with the prompt are passed to the OpenAI Chat Completion API. The GPT model then determines which function best matches the prompt and populates a JSON object using the function JSON schema and prompt data. If there is a successful match, the chat completion API returns the function name and the JSON object/entity.

It's important to note that the model doesn't magically call the function on your behalf, that's your codes job, you are returned a function name and arguments and it's up to your code to determine what to do with the data. You can read more about OpenAI Functions in the [OpenAI Functions documentation](https://platform.openai.com/docs/guides/gpt/function-calling).

### OpenAI Function Examples

Here are two examples of OpenAI Functions. Take a moment to review the following JSON OpenAI Function definitions, you'll see a function name, description, parameters and and a series of properties that describe the function and its schema. You can define and pass multiple function definitions to the OpenAI Chat Completion API.

```json
{
    "name": "get_current_weather",
    "description": "Get the current weather in a given location",
    "parameters": {
        "type": "object",
        "properties": {
            "location": {
                "type": "string",
                "description": "The city and state, e.g. San Francisco, CA"
            },
            "unit": {
                "type": "string",
                "enum": ["celsius", "fahrenheit"]
            }
        },
        "required": ["location"]
    }
}
```

```json
light_state = {
    "name": "set_light_state",
    "description": "Turn a light on or off and sets it to a given color and brightness",
    "parameters": {
        "type": "object",
        "properties": {
            "device": {
                "type": "string",
                "description": "The name of the light"
            },
            "state": {
                "type": "string",
                "enum": ["on", "off"]
            },
            "brightness": {
                "type": "string",
                "enum": ["low", "medium", "high"]
            },
            "color": {
                "type": "string",
                "enum": ["red", "white", "blue", "green", "yellow", "purple", "orange", "pink", "cyan", "magenta", "lime", "indigo", "teal", "olive", "brown", "black", "grey", "silver", "gold", "bronze", "platinum", "rainbow"]
            }
        },
        "required": ["device"]
    }
}
```

## Home assistant OpenAI Functions

This home assistant uses the following OpenAI Functions:

- get_current_weather: Weather data is from https://www.weatherapi.com/ and is used to ground a GPT prompts.
- light_state: for controlling imaginary lights
- washing_machine_state: for controlling imaginary washing machines
- lock_state: for controlling imaginary locks

### How the code works

<!-- The code defines the role prompts, a list of OpenAI Functions, the temperature, and maximum number of tokens. The `openai_functions`variable contains a list of all the OpenAI Function definitions.  -->

When the `openai.ChatCompletion.create` function is called, 

- The `openai_functions` variable is passed to the `functions` parameter. The `functions` parameter is a list of OpenAI Function definitions. 
- The `messages` parameter is a list of messages that are passed to the GPT model. The `messages` parameter contains the role, and content of the message. 
- The `role` parameter is either `system`, `user`, or `assistant`. The `content` parameter is the message text. 
- The `temperature` parameter is the temperature of the GPT model. 
- The `max_tokens` parameter is the maximum number of tokens to return.


To learn more about OpenAI Functions, see the [OpenAI Functions documentation](https://platform.openai.com/docs/guides/gpt/function-calling).

```python
response_1 = openai.ChatCompletion.create(
    model="gpt-3.5-turbo-0613",
    messages=[
        {"role": "system", "content": "You are a home automation assistant and you can only help with home automation."},
        {"role": "system", "content": "Start all responses with 'I'm a home automation assistant'."},
        {"role": "system", "content": "Device types limited to those listed in functions. Ask for the device name unsure. Device names have no spaces."},
        {"role": "system", "content": "Only use the functions you have been provided with."},
        {"role": "assistant", "content": last_assistant_message},
        {"role": "user", "content": text},
    ],
    functions=openai_functions,
    temperature=0.0,
    max_tokens=OPENAI_MAX_TOKENS,
)
```

### Parsing the response

The `openai.ChatCompletion.create` function returns a `response` object. 
- The `response` object contains the `choices` object. 
- The `choices` object contains the `message` of the response. 
- If a function is matched, a `function_call` object is returned in the `message` object. The `function_call` object contains the function `name` and `arguments` object.

```python
result = response_1.get('choices')[0].get('message')
content = result.get("content", "")

if result.get("function_call"):
    function_name = result.get("function_call").get("name")
    arguments = json.loads(result.get("function_call").get("arguments"))
```