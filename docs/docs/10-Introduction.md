---
sidebar_position: 0
slug: /
title: "Azure OpenAI Proxy Service"
---

import Social from '@site/src/components/social';

<Social
    page_url="https://github.com/gloveboxes/azure-openai-service-proxy"
    image_url="https://github.com/gloveboxes/azure-openai-service-proxy/raw/main/docs/static/img/openai_proxy_banner.jpeg"
    title="Azure OpenAI Proxy Service"
    description= "🏭 Get started with Azure OpenAI Proxy Service - Azure OpenAI Hacks Made Easy"
    hashtags="OpenAI"
    hashtag=""
/>

![](../static/img/openai_proxy_banner.jpeg)

## Introduction to the OpenAI Proxy

The goal of the Azure OpenAI proxy service is to simplify access to an Azure OpenAI `Playground-like` experience and supports Azure OpenAI SDKs, LangChain, and REST endpoints for developer events, workshops, and hackathons. Access is granted using a timebound `event code`.

An `event code` is typically the name of an event, eg `hackathon`, and is given to the event attendees. The event administrator sets the period the `event code` will be active.

There are two primary use cases for the Azure OpenAI proxy service:

1. Access to an Azure OpenAI Web `Playground-like` experience for developers to explore the Azure OpenAI chat completion using a timebound event code.
2. The proxy service is designed to be a drop in replacement for the the most commonly used OpenAI REST APIs, access is granted using a timebound event code. It is compatible and interoperable with the OpenAI SDKs. Initially, the proxy service supports the `chat completion`, `completion` and `embeddings` APIs. It is compatible and interoperable with the OpenAI SDKs and initial testing has been done with the Python OpenAI SDKs and LangChain.

## OpenAI Proxy Playground

The Azure OpenAI proxy service provides a `Playground-like` experience for developers to explore the Azure OpenAI chat completion using the timebound event code.

![OpenAI Proxy Playground](media/openai_proxy_playground.png)
