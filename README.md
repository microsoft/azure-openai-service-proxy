# Azure OpenAI Proxy

The Azure OpenAI Proxy is a simple API that allows you to use the OpenAI API without having to expose your API key to the client. It is designed to be deployed to Azure Container Apps, which provides a managed environment for running containerized apps without having to manage the underlying infrastructure. The target use case is for hackathons and other time-bound events where you want to provide access to the OpenAI API without having to worry about exposing your API key.

The solution consists of two parts, a REST API, and a web client with a similar look and feel to the official Azure OpenAI Playground. The REST API is a simple Python FastAPI app that proxies requests to the OpenAI API. The web client is a simple React app that allows you to test the API.

## Setup

This repo is set up for deployment on Azure Container Apps using the configuration files in the `infra` folder.

### Prerequisites

1. An Azure subscription
2. Deployed Azure OpenAI Models

### OpenAI Deployments

Before you can deploy the REST API you need to have deployed the OpenAI models you want to use. Once deployed, you need to create a deployment configuration string. You can declare multiple OpenAI deployments and they are used to load balance OpenAI Chat requests using a simple [round robin](https://en.wikipedia.org/wiki/Round-robin_scheduling) schedule. It's important to ensure there are no spaces in the configuration string as it will cause the deployment to fail. The deployment configuration string format is a JSON array of objects formated as follows:



```text
[{"resource_name":"<Your Azure OpenAI resource name>","endpoint_key":"<Your resource name endpoint key>","deployment_name","<Your OpenAI model deploymentname>"}]
```

#### Tips

1. When deploying with a different capacity, you can use the same deployment name multiple times, and the REST API will automatically load balance across the different capacity deployments. For example, you have one OpenAI deployment with a capacity of 600K requests per minute, and another deployment of 300. Add the 600K deployment to the configuration string twice so it will be called twice as often as the smaller deployment.
2. Keep the deployment connection string in a safe place as it contains your OpenAI deployment keys.
3. You can also update the pool of OpenAI deployments by updating the `OPENAI_DEPLOYMENT_CONFIG` environment variable in the Azure Container App using the `az containerapp update` command. For example:

```shell
az containerapp update -n YOUR_CONTAINER_APP_NAME -g YOUR_RESOURCE_GROUP  --subscription YOUR_SUBSCRIPTION_ID --set-env-vars AZURE_OPENAI_DEPLOYMENTS='YOUR_OPENAI_DEPLOYMENT_CONFIGURATION_JSON_STRING'
```

### Deploying

Steps for deployment:

The recommeded way to deploy this app is with Dev Containers. Install the [VS Code Remote Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) and Docker, open this repository in a container and you'll be ready to go.


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
    It will prompt you to provide an `azd` environment name (like "fastapi-app"), select a subscription from your Azure account, and select a location (like "eastus"). Then it will provision the resources in your account and deploy the latest code. If you get an error with deployment, changing the location can help, as there may be availability constraints for some of the resources.

1. When `azd` has finished deploying, you'll see the REST endpoint docs URI in the command output. Visit that URI, and you should see the REST API docs! 🎉
1. If make any changes to the app code, just run:

    ```shell
    azd deploy
    ```

## Time bound event authorisation

Access to the REST endpoint is controller by an event code. The REST endpont is accessible when the current UTC time is between the StartUTC and the EndUTC times and the event is active. The event code is passed in the `openai-event-code` header. If the event code is not passed, or the event is not active, or the current UTC time is not between the StartUTC and the EndUTC times, the REST endpoint will return a `401` unauthorized error.

Event details are stored in an Azure Storage account table named `playgroundauthorization`. This table is created when the app is deployed and starts. The table has the following schema:

| Property     | Type     | Description                                 |
| ------------ | -------- | ------------------------------------------- |
| PartitionKey | string   | Must be 'playground'                        |
| RowKey       | string   | The event code. For example: myevent2022    |
| Active       | boolean  | Is the event active, true or false          |
| StartUTC     | datetime | The start date and time of the event in UTC |
| EndUTC       | datetime | The end date and time of the event in UTC   |
| EventName    | string   | The name of the event                       |
| ContactName  | string   | The name of the event contact               |
| ContactEmail | string   | The email address of the event contact      |

### Adding an event

For now, you add an event via the Azure Storage Account `Storage browser`. The `Storage browser` is available in the Azure Portal, under the `Storage account` resource. 

1. Select the Azure Storage Account resource, then select `Storage explorer (preview)` from the left-hand menu, then select `Tables` from the left-hand menu, and finally select the `playgroundauthorization` table. Add an entry using the above schema, noting that the `PartitionKey` must be `playground` and the column names are case sensitive, and you must enter dates in [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601) format in UTC. The [worldtimebuddy](https://www.worldtimebuddy.com) is a great time resource to convert your local time to UTC. 

Here is an example

```text
PartitionKey: playground
RowKey: myevent2022
Active: true
StartUTC: 2023-10-01T00:00:00Z
EndUTC: 2023-10-02T00:00:00Z
EventName: My Event 2023
ContactName: John Smith
ContactEmail: jsmith@example.com
```

### Event Code Cache

Event data, namely the `EventCode`, `StartUTC`, and `EndUTC` are cached in the REST API. The cache is refreshed every 10 minutes. This is to reduce the number of calls to the Azure Storage Account table. The implication of this is that if you update the event details in the Azure Storage Account table, it may take up to 10 minutes for the changes to be reflected in the REST API.

### Testing the REST API endpoint

There are various options to test the endpoint. The simpliest is to use Curl from either PowerShell or a Bash/zsh terminal. For example:

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

There are a number of load testing tools available. The recommended tool is JMeter as the test plan can be deployed to Azure. The JMeter test plan is located in the `loadtest` folder. The test plan is configured to run 100 concurrent users, generating 4 requests per minute.

1. You'll need to update the URL in the `HTTP Request Defaults` element to point to your REST API endpoint.
   
    ![update url](./docs/media/jmeter_requests.png)

2. You'll need to update the `HTTP Header Manager` element to include your event code.
   
    ![update event code](./docs/media/jmeter-request-header.png)

### Example load test

![](./docs/media/example_perf_jmeter.png)