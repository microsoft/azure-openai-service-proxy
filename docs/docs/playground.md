# The AI Proxy Playground

The Azure AI Playground is a web-based application that provides a `Playground` experience for developers to explore the Azure OpenAI chat completion using a time bound event code with different models and parameters.

The AI Playground provides a simple and intuitive interface to interact with the Azure OpenAI chat completion API. The AI Playground is designed to be used in a workshop or event setting, where attendees can explore the Azure OpenAI chat completion using a time bound event code with different models and parameters.

The AI Playground provides the following features:

1. **Model selection**: The AI Playground allows you to select from a list of available models. The models are configured by the event organizer and are available to the attendees of the event.

    ![OpenAI Proxy Playground](media/openai_proxy_playground.png)

1. Set the `Max Token` parameter: The `Max Token` parameter is used to control the length of the chat completion.
1. Set the `Temperature` parameter: The `Temperature` parameter is used to control the randomness of the chat completion.
1. Set the `Top P` parameter: The `Top P` parameter is used to control the diversity of the chat completion.

## Azure Chat Completions Functions

Attendees can explore Azure OpenAI Chat Completions Functions using the AI Playground. Simply paste an Azure Function, select the function from the `OpenAI Functions` dropdown. The AI Playground will call the Azure OpenAI Chat Completions API with the selected function and display the results.
