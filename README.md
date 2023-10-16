# Azure OpenAI Proxy

The Azure OpenAI Proxy is a simple API that allows you to use the OpenAI API without having to expose your API key to the client. It is designed to be deployed to Azure Container Apps, which provides a managed environment for running containerized apps without having to manage the underlying infrastructure. The target use case is for hackathons and other time-bound events where you want to provide access to the OpenAI API without having to worry about exposing your API key.

The solution consists of two parts, a REST API, and a web client with a similar look and feel to the official Azure OpenAI Playground. The REST API is a simple Python FastAPI app that proxies requests to the OpenAI API. The web client is a simple React app that allows you to test the API.

## Setup

This repo is set up for deployment on Azure Container Apps using the configuration files in the `infra` folder.

### Prerequisites

1. An Azure subscription
2. Deployed Azure OpenAI Models

### Required software

Tested on Windows, macOS and Ubuntu 22.04.

Install:

1. [VS Code](https://code.visualstudio.com/)
2. [VS Code Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
3. [Docker](https://www.docker.com/products/docker-desktop)

## Deploying

The recommended way to deploy this app is with Dev Containers. Install the [VS Code Remote Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) and Docker, open this repository in a container and you'll be ready to go.

1. Clone the repo:

    ```shell
    git clone https://github.com/gloveboxes/azure-openai-service-proxy.git
    ```

1. Using VS Code, open the `azure-openai-service-proxy/src/endpoint/simple-fastapi-container` folder:

1. You will be prompted to `Reopen in Container`, click the button to do so.

1. Login to Azure:

    ```shell
    azd auth login
    ```

1. Provision and deploy all the resources:

    ```shell
    azd up
    ```

    It will prompt you to provide an `azd` environment name (like "openai-proxy"), select a subscription from your Azure account, and select a location (like "eastus"). Then it will provision the resources in your account and deploy the latest code. If you get an error with deployment, changing the location can help, as there may be availability constraints for some of the resources.
    

    On completion, the following Azure resources will be provisioned:

      ![](./docs/media/azure_resources.png)

1. When `azd` has finished deploying you'll see a link to the Azure Resource Group created for the solution.
1. To make any changes to the app code, just run:

    ```shell
    azd deploy
    ```

## Time-bound event authorization

Access to the REST endpoint is controlled by an event code. The REST endpoint is accessible when the current UTC is between the StartUTC and the EndUTC times and the event is active. The event code is passed in the `openai-event-code` header. If the event code is not passed, or the event code is not active, or the current UTC is not between the StartUTC and the EndUTC times, the REST endpoint will return a `401` unauthorized error.

Event code details are stored in an Azure Storage account table named `playgroundauthorization`. This table is created when the app is deployed and starts. The table has the following schema:

| Property     | Type     | Description                                                                                                                                                                                                                                                                                                        |
| ------------ | -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| PartitionKey | string   | Must be 'playground'                                                                                                                                                                                                                                                                                               |
| RowKey       | string   | The event code must be between 6 and 20 characters long. For example myevent2022. </br>Note, you can't use the following characters in the event name: 'The forward slash (/), backslash (\\), number sign (#), and question mark (?) characters' as they aren't allowed for an Azure Storage Table RowKey property. |
| Active       | boolean  | Is the event active, true, or false                                                                                                                                                                                                                                                                                 |
| MaxTokenCap  | int      | The maximum number of tokens per request. This overrides the user set Max Token value for load balancing                                                                                                                                                                                                            |
| StartUTC     | datetime | The start date and time of the event in UTC                                                                                                                                                                                                                                                                        |
| EndUTC       | datetime | The end date and time of the event in UTC                                                                                                                                                                                                                                                                          |
| EventName    | string   | The name of the event                                                                                                                                                                                                                                                                                              |
| ContactName  | string   | The name of the event contact                                                                                                                                                                                                                                                                                      |
| ContactEmail | string   | The email address of the event contact                                                                                                                                                                                                                                                                             |

### Adding an event

For now, you add an event via the Azure Storage Account `Storage browser`. The `Storage browser` is available in the Azure Portal, under the `Storage account` resource.

1. Select the Azure Storage Account resource, then select `Storage explorer (preview)` from the left-hand menu, then select `Tables` from the left-hand menu.
1. Next, select the `playgroundauthorization` table. 
2. Add an entry using the above schema, noting that the `PartitionKey` must be set to `playground` and the column names are case sensitive, and you must enter dates in [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601) format in UTC. The [worldtimebuddy](https://www.worldtimebuddy.com) is a great time resource to convert your local time to UTC.

Here is an example

```text
PartitionKey: playground
RowKey: myevent2022
Active: true
MaxTokenCap: 1024
StartUTC: 2023-10-01T00:00:00Z
EndUTC: 2023-10-02T00:00:00Z
EventName: My Event 2023
ContactName: John Smith
ContactEmail: jsmith@example.com
```

### Event Code Cache

Event data, namely the `EventCode`, `StartUTC`, `EndUTC`, and `MaxTokenCap` are cached in the REST API. The cache is refreshed every 10 minutes. Caching is to reduce the number of calls to the Azure Storage Account table. Because of caching, it may take up to 10 minutes for the changes to be reflected in the REST API.


## Azure OpenAI Deployments

### Create an Azure OpenAI deployment

From the Azure Portal, select the Azure OpenAI resource, then select the `Deployments` tab, and finally select `Create deployment`. Enter a friendly name for the deployment, and select the model and the capacity. The capacity is the number of requests per minute. The capacity can be changed at any time. The deployment will take a few minutes to provision.

Make a note of the following as you'll need them for the next step:

1. The `Deployment name`.
2. The `Service location`.
3. The `Endpoint key`.

### Configuring the Azure OpenAI deployments

The REST API is designed to load balance across multiple Azure OpenAI deployments. The deployment configuration is loaded from the 'configuration' Azure Storage Account table. The table has the following schema:

| Property       | Type    | Description                                             |
| -------------- | ------- | ------------------------------------------------------- |
| PartitionKey   | string  | Must be 'openai-chat'                                   |
| RowKey         | string  | The deployment-friendly name                            |
| Location       | string  | The Azure region where the OpenAI deployment is located |
| EndpointKey    | string  | The Azure OpenAI deployment key                         |
| DeploymentName | string  | The Azure OpenAI deployment name                        |
| Active         | boolean | Is the deployment active, true, or false                 |

Ideally, the deployments should be of similar TPM (Tokens Per Minute) capacity. The REST API will load balance across the deployments using a simple [round robin](https://en.wikipedia.org/wiki/Round-robin_scheduling) schedule. The REST API will only load balance across active deployments. If there are no active deployments, the REST API will return a `500` service unavailable error.

If one deployment is of greater capacity than another, you can add the deployment to the configuration table multiple times. For example, if you have one deployment with a capacity of 600K requests per minute, and another deployment of 300. Add the 600K deployment to the configuration table twice so it will be called twice as often as the smaller deployment.

## Scaling the REST API

The REST API is stateless, so it can be scaled horizontally. The REST API is designed to auto-scale up and down using Azure Container Apps replicas. The REST API is configured to scale up to 10 replicas. The number of replicas can be changed from the Azure Portal or from the az cli. For example, to scale to 30 replicas using the az cli, change the:

```shell
az containerapp update -n $APP_NAME -g $RESOURCE_GROUP --subscription $SUBSCRIPTION_ID --replica 30
```

## Understanding OpenAI Rate Limits and the MaxTokenCap

Azure OpenAI model deployments have two limits, the first being tokens per minute, and the second being requests per minute. You are most likely to hit the Tokens per minute limit especially as you scale up the number of users using the system.

Tokens per minute is the total number of tokens you can generate per minute. The number of tokens per call to OpenAI Chat API is the sum of the Max Token parameter, plus the tokens that make up your msg (system, assistant, and user), plus best_of parameter setting.

For example, you have a model deployment rated at 500K tokens per minute.

![](docs/media/rate_limits.png)

If users set the Max Token parameter to 2048, with a message of 200 tokens, and you had 100 concurrent users sending on average 6 messages per minute then the total number of tokens per minute would be (2048 + 200) * 6 * 100) = 1348800 tokens per minute. This is well over the 500K tokens per minute limit of the Azure OpenAI model deployment and the system would be rate-limited and the user experience would be poor.

This is where the MaxTokenCap is useful for an event. The MaxTokenCap is the maximum number of tokens per request. This overrides the user's Max Token request for load balancing. For example, if you set the MaxTokenCap to 512, then the total number of tokens per minute would be (512 + 200) * 6 * 100) = 427200 tokens per minute. This is well under the 500K tokens per minute limit of the Azure OpenAI model deployment and will result in a better experience for everyone as it minimizes the chance of hitting the rate limit across the system.

MaxTokenCap is set at the event level and is configured in the Azure Storage Account table named `playgroundauthorization`. See the section above on [adding an event](#adding-an-event) for more details.

### Testing the REST API endpoint

There are various options to test the endpoint. The simplest is to use Curl from either PowerShell or a Bash/zsh terminal. For example:

From PowerShell 7 and above on Windows, macOS, and Linux:

```pwsh
curl -X 'POST' `
  'https://YOUR_REST_ENDPOINT/api/oai_prompt' `
  -H 'accept: application/json' `
  -H 'Content-Type: application/json' `
  -H 'openai-event-code: YOUREVENTCODE' `
  -d '{
  "messages": [
    {"role": "system", "content":"What is this about"},
    {"role": "user", "content":"The quick brown fox jumps over the lazy dog"}
  ],
  "max_tokens": 1024,
  "temperature": 0,
  "top_p": 0,
  "stop_sequence": "string",
  "frequency_penalty": 0,
  "presence_penalty": 0
}'  | ConvertFrom-Json | ConvertTo-Json
```

From Bash/zsh on macOS, Linux, and Windows WSL:

```bash
curl -X 'POST' \
  'https://YOUR_REST_ENDPOINT/api/oai_prompt' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -H 'openai-event-code: YOUREVENTCODE' \
  -d '{
  "messages": [
    {"role": "system", "content":"What is this about"},
    {"role": "user", "content":"The quick brown fox jumps over the lazy dog"}
  ],
  "max_tokens": 1024,
  "temperature": 0,
  "top_p": 0,
  "stop_sequence": "string",
  "frequency_penalty": 0,
  "presence_penalty": 0
}'
```

Better still, pip or brew install `jq` to pretty print the JSON response.

```bash
curl -X 'POST' \
  'https://YOUR_REST_ENDPOINT/api/oai_prompt' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -H 'openai-event-code: YOUREVENTCODE' \
  -d '{
  "messages": [
    {"role": "system", "content":"What is this about"},
    {"role": "user", "content":"The quick brown fox jumps over the lazy dog"}
  ],
  "max_tokens": 1024,
  "temperature": 0,
  "top_p": 0,
  "stop_sequence": "string",
  "frequency_penalty": 0,
  "presence_penalty": 0
}' | jq "."
```

## Load testing

There are several load testing tools available. The recommended tool is JMeter as the test plan can be deployed to Azure. The JMeter test plan is located in the `loadtest` folder. The test plan is configured to run 100 concurrent users, generating 4 requests per minute.

1. You'll need to update the URL in the `HTTP Request Defaults` element to point to your REST API endpoint.

    ![update url](./docs/media/jmeter_requests.png)

2. You'll need to update the `HTTP Header Manager` element to include your event code.

    ![update event code](./docs/media/jmeter-request-header.png)

### Example load test

![](./docs/media/example_perf_jmeter.png)
