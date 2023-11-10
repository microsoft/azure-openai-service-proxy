# OpenAI model deployments

From the Azure Portal, select the Azure OpenAI resource, then select the `Deployments` tab, and finally select `Create deployment`. Enter a friendly name for the deployment, and select the model and the capacity. The capacity is the number of requests per minute. The capacity can be changed at any time. The deployment will take a few minutes to provision.

Make a note of the following as you'll need them for the next step:

1. The `Deployment name`.
2. The `Service location`.
3. The `Endpoint key`.

## Configuring the Azure OpenAI deployments

The proxy service is designed to load balance across multiple Azure OpenAI deployments. The deployment configuration is loaded from the 'configuration' Azure Storage Account table. The table has the following schema:

| Property       | Type    | Description                                             |
| -------------- | ------- | ------------------------------------------------------- |
| PartitionKey   | string  | Must be 'openai-chat'                                   |
| RowKey         | string  | The deployment-friendly name                            |
| Location       | string  | The Azure region where the OpenAI deployment is located |
| EndpointKey    | string  | The Azure OpenAI deployment key                         |
| DeploymentName | string  | The Azure OpenAI deployment name                        |
| Active         | boolean | Is the deployment active, true, or false                 |

Here is an example

```text
PartitionKey: openai-chat
RowKey: gpt-35-turbo-01
Location: swedencentral
EndpointKey: wd7w6d77w868sd678s
DeploymentName: gpt-35-turbo
Active: true
```


Ideally, the deployments should be similar TPM (Tokens Per Minute) capacity. The proxy service will load balance across the deployments using a simple [round robin](https://en.wikipedia.org/wiki/Round-robin_scheduling) scheduler. The proxy service will only load balance across active deployments. If there are no active deployments, the proxy service will return a `500` service unavailable error.

If one deployment is of greater capacity than another, you can add the deployment to the configuration table multiple times. For example, if you have one deployment with a capacity of 600K requests per minute, and another deployment of 300. Add the 600K deployment to the configuration table twice so it will be called twice as often as the smaller deployment.