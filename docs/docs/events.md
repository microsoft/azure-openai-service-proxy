# Setting up events

Once you have configured the resources, you can use the same resources for multiple events. This guide will walk you through the process of setting up events.

## Setting up events

From the AI Proxy Admin portal, you can create and manage events. An event is a time bound access to the AI Proxy service.

1. Sign into the AI Proxy Admin portal and authenticate using your organization's Entra credentials.
1. Select the `Events` tab, then add a new event.

    ![](./media/proxy-events.png)

1. Add the event details, including the event name, start and end date.

    ![](./media/proxy-new-event.png)

## Understand event options

1. Max Token Cap: The maximum number of tokens that can be used for a chat completion or completion API call. The `Max Token Cap` overrides the user set Max Token parameter in the API call and is used to limit and balance access to the Azure OpenAI resources for all attendees of the event.
