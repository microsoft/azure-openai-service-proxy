# Using the Playground

The Playground is a web-based application that allows users to experiment with OpenAI Chat Completions. 

## Authentication

An event attendee needs to authenticate to the Playground using an event code. The event code is validated and the current time is checked against the event start and end times. If the event code is valid and the event is active, then the attendee is allowed to use the Playground.

The Azure OpenAI proxy service provides a `Playground like` experience for developers to explore the Azure OpenAI chat completion using the time bound event code.

![](media/openai_proxy_playground.png)

<!-- :::tip

The event start time and end time are in UTC (Universal Coordinated Time).

::: -->


