# The Management API

There is a Management API for adding events and listing events. The API is secured with a Management ID token. The Management ID token is stored in the Azure Storage Account `management` table. The `management` table is created when the proxy service is deployed and started.

For now, the only way to manage events is via the Management API. In the future, there may be a web UI for managing events.

## Adding events

The following is an example of a `curl` command to add an event to the system.

```shell
curl -X POST -H "Content-Type: application/json" -H "Authorization: Bearer YOUR_MANAGEMENT_ID_TOKEN" -d '{
    "event_name": ".NET OpenAI Hack",
    "start_utc" : "2023-11-16T00:00:00",
    "end_utc": "2023-12-16T00:00:00",
    "max_token_cap": 2048,
    "event_url": "http://www.example.com/event_name",
    "event_url_text": "Join the .NET OpenAI Hack",
    "organizer_name": "Ant Blogs",
    "organizer_email": "ablogs@example.com"
}' https://YOUR_OPENAI_PROXY_ENDPOINT/v1/api/management/addevent | jq
```

## Listing events

For now, you can list all events or all active events.

### List all events

The following is an example of a `curl` command to list all events in the system.

```shell
curl -X GET -H "Authorization: Bearer YOUR_MANAGEMENT_ID_TOKEN" https://YOUR_OPENAI_PROXY_ENDPOINT/v1/api/management/listevents/all | jq
```

### List active events

An active event is an event where the current UTC time is is between the event `StartUTC` and `EndUTC` times and the event is active. The API is secured with a Management ID token. The Management ID token is stored in the Azure Storage Account `management` table. The `management` table is created when the proxy service is deployed and started.

The following is an example of a `curl` command to list all active events in the system.

```shell
curl -X GET -H "Authorization: Bearer YOUR_MANAGEMENT_ID_TOKEN" YOUR_OPENAI_PROXY_ENDPOINT/v1/api/management/listevents/active | jq
```

## Event Code Cache

Event data, namely the `EventCode`, `StartUTC`, `EndUTC`, and `MaxTokenCap` are cached by the proxy service. The cache is refreshed every 10 minutes. Caching is implemented to improve performance by reducing the number of calls to the Azure Storage Account table. Because of caching, it may take up to 10 minutes for the changes to be reflected in the proxy service.

## Upserting Azure OpenAI Model Deployments

There a Management API for adding Azure OpenAI model deployments to the system. The API is secured with a Management ID token. The Management ID token is stored in the Azure Storage Account `management` table. The `management` table is created when the proxy service is deployed and started.

```shell
curl -X POST -H "Content-Type: application/json" -H "Authorization: Bearer YOUR_MANAGEMENT_ID_TOKEN" -d '{
    "deployment_class": "openai-chat",
    "friendly_name" : "chat-glovebox",
    "deployment_name": "turbo-gpt",
    "endpoint_key": "1234455",
    "resource_name": "my-resource-name",
    "active": false
}' https://YOUR_OPENAI_PROXY_ENDPOINT/v1/api/management/modeldeployment/upsert
```

## Listing Azure OpenAI Model Deployments

You can list all Azure OpenAI model deployments or all active Azure OpenAI model deployments. The API is secured with a Management ID token. The Management ID token is stored in the Azure Storage Account `management` table. The `management` table is created when the proxy service is deployed and started.

### List all Azure OpenAI model deployments

The following is an example of a `curl` command to list all Azure OpenAI model deployments in the system.

```shell
https://YOUR_OPENAI_PROXY_ENDPOINT/v1/api/management/modeldeployment/list/all
curl -X GET -H "Authorization: Bearer YOUR_MANAGEMENT_ID_TOKEN" https://YOUR_OPENAI_PROXY_ENDPOINT/v1/api/management/listevents/all | jq
```

### List active Azure OpenAI model deployments

An active Azure OpenAI model deployment is an Azure OpenAI model deployment where the `active` property is set to `true`.

```shell
curl -X GET -H "Authorization: Bearer YOUR_MANAGEMENT_ID_TOKEN" https://YOUR_OPENAI_PROXY_ENDPOINT/v1/api/management/listevents/active | jq
```
