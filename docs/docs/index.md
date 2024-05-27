# Azure AI Proxy

![](media/openai_proxy_banner.jpeg)

## Introduction to the Azure AI Proxy

The goal of the Azure OpenAI proxy service is to simplify access to an AI `Playground` experience, support for Azure OpenAI SDKs, LangChain, and REST endpoints for developer events, workshops, and hackathons. Access is granted using a time bound `API Key`.

There are four primary use cases for the Azure OpenAI proxy service:

1. Access to an AI `Playground` experience for developers to explore the Azure OpenAI chat completion using a time bound event code and different models and parameters.
2. Access to developer APIs via REST endpoints and the OpenAI SDKs and LangChain. Access to these services is granted using a time bound event code. Initially, the proxy service supports the `chat completion`, `completion`, `embeddings`, and `dall-e-3` APIs.
3. Access to Azure AI Search queries using the Azure OpenAI proxy service. Access to these services is granted using a time bound event code.
4. You are running a hackathon and users can't provision their own Azure OpenAI resources as they don't have a corporate email address.

## OpenAI Proxy Playground

The Azure OpenAI proxy service provides a `Playground-like` experience for developers to explore the Azure OpenAI chat completion using the time bound event code with different models and parameters.

![OpenAI Proxy Playground](media/openai_proxy_playground.png)

## Azure AI Proxy Architecture

The Azure AI Proxy consists of the following components:

1. Self-service event management. Configure and manage events and resources for the events.
1. Self-service attendee registration. Attendees can register for an event and receive a time bound API Key to access the AI Proxy service.
1. The AI Proxy service. The AI Proxy service provides access to the Azure AI resources using a time bound API Key.
1. The AI Playground. The AI Playground provides an AI `Playground` experience for developers to explore the Azure OpenAI chat completion using the time bound event code with different models and parameters.
