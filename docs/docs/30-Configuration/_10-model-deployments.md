# Managing models

## The Management API

There is a Management API for adding, updating, and list model deployments. The API is secured with a Management ID token. The Management ID token is stored in the Azure Storage Account `management` table. The `management` table is created when the proxy service is deployed and started.

For now, the only way to manage model depolyments is via the Management API. In the future, there may be a web UI for managing model deployments.

## Adding or updating as Azure OpenAI Model Deployment

There a Management API for adding Azure OpenAI model deployments to the system. The API is secured with a Management ID token. The Management ID token is stored in the Azure Storage Account `management` table. The `management` table is created when the proxy service is deployed and started.

### Model deployment classes

The following is a list of the valid deployment classes supported by the Azure OpenAI proxy service.

| Model deployment class      | Models                                                                                                   | Description                                                                     |
| --------------------------- | -------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| `openai-chat`               | gpt-35-turbo, gpt-35-turbo-16k, or newer                                                                 | This is the model deployment class for the Azure OpenAI Chat Completions API.   |
| `openai-completions`        | davinci-002 or newer                                                                                     | This is the model deployment class for the Azure OpenAI Completions API.        |
| `openai-embeddings`         | text-embedding-ada-002 or newer                                                                          | This is the model deployment class for the Azure OpenAI Embeddings API.         |
| `openai-images-generations` | No model is deploy, just an Azure OpenAI resource in a location that supports the Images Generations API | This is the model deployment class for the Azure OpenAI Images Generations API. |

### Load balancing

You can deploy multiple models of the same model deployment class. For example, you can deploy multiple `gpt-35-turbo` models, you'd give them different `friendly_name` and `deployment_name` values. The proxy will round robin across the models of the same model deployment class to balance the load.

## Adding an Azure OpenAI model deployment

```shell
curl -X PATCH -H "Content-Type: application/json" -H "Authorization: Bearer YOUR_MANAGEMENT_ID_TOKEN" -d '{
    "deployment_class": "A_VALID_MODEL_DEPLOYMENT_CLASS",
    "friendly_name" : "YOUR_AZURE_OPENAI_DEPLOYMENT_FRIENDLY_NAME",
    "deployment_name": "YOUR_AZURE_OPENAI_DEPLOYMENT_NAME",
    "endpoint_key": "YOUR_AZURE_OPENAI_ENDPOINT_KEY",
    "resource_name": "YOUR_AZURE_OPENAI_RESOURCE_NAME",
    "active": true
}' https://YOUR_OPENAI_PROXY_ENDPOINT/api/v1/management/modeldeployment/upsert
```

## Listing Azure OpenAI Model Deployments

You can list all Azure OpenAI model deployments or all active Azure OpenAI model deployments. The API is secured with a Management ID token. The Management ID token is stored in the Azure Storage Account `management` table. The `management` table is created when the proxy service is deployed and started.

### List all Azure OpenAI model deployments

The following is an example of a `cURL` command to list all Azure OpenAI model deployments in the system.

```shell
https://YOUR_OPENAI_PROXY_ENDPOINT/api/v1/management/modeldeployment/list/all
curl -X GET -H "Authorization: Bearer YOUR_MANAGEMENT_ID_TOKEN" https://YOUR_OPENAI_PROXY_ENDPOINT/api/v1/management/listevents/all | jq
```

### List active Azure OpenAI model deployments

An active Azure OpenAI model deployment is an Azure OpenAI model deployment where the `active` property is set to `true`.

```shell
curl -X GET -H "Authorization: Bearer YOUR_MANAGEMENT_ID_TOKEN" https://YOUR_OPENAI_PROXY_ENDPOINT/api/v1/management/listevents/active | jq
```
