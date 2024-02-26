# The Playground

The Azure OpenAI proxy service provides a `Playground like` experience for developers to explore the Azure OpenAI chat completion using the time bound API Key. The Playground is UX modeled on the official Azure OpenAI Playground, so if you've used the official Playground, you'll be familiar with the Playground.

## Playground Authentication

Access the Playground is with an API Key. The API Key is validated and the current time is checked against the event start and end times of the event. If the API Key is valid and the event is active, then the attendee is allowed to use the Playground.

![The Azure OpenAI Playground-like experience](media/openai_proxy_playground.png)

## Opening the Playground

To open the Playground, open a browser and navigate to the following URL, replacing `YOUR_OPENAI_PROXY_ENDPOINT` with the URL of the Azure OpenAI proxy service.
