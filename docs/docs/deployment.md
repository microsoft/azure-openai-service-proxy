# OpenAI proxy service

The solution consists of three parts; the proxy service, the proxy playground, with a similar look and feel to the official Azure OpenAI Playground, and event admin.

## Deployment issues

1. Entra app registration.
1. Postgres requires manual registration of the `pgcrypto` extension.
1. Deploying the AI Proxy Admin Portal does not work on macOS on Apple Silicon. The workaround for now is to deploy the admin portal on a Windows, Linux machine, or from GitHub Codespaces.

## Setup

This repo is set up for deployment on Azure Container Apps using the configuration files in the `infra` folder.

### Prerequisites

1. An Azure subscription
2. Deployed Azure OpenAI Models

### Required software

<!-- Tested on Windows, macOS and Ubuntu 22.04.

Install:

1. [VS Code](https://code.visualstudio.com/)
2. [VS Code Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
3. [Docker](https://www.docker.com/products/docker-desktop) -->

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
    azd auth login --use-device-code
    ```

    ```shell
    az login --use-device-code
    ```

1. Entra app registration

    The AI Proxy admin is secured using Entra. You first need to register an application in your organizations Entra directory.

    1. Navigate to [entra.microsoft.com](https://entra.microsoft.com)
    1. Select **Application**, then register an application.
    1. Name the registration, select web type, single tenant, enable **token IDs**
    1. Save
    1. Navigate to **overview**, and make a note of the client ID as you will need it when you deploy the solution.

1. Provision and deploy all the resources:

    ```shell
    azd up
    ```

    It will prompt you to provide an `azd` environment name (like "aiproxy"), select a subscription from your Azure account, and select a location (like "eastus" or "sweden central"). Then azd will provision the resources in your account and deploy the latest code. If you get an error with deployment, changing the location can help, as there may be availability constraints for some of the resources.

    On completion, the following Azure resources will be provisioned:

    ![Azure OpenAI Playground experience](media/azure_resources.png)

1. When `azd` has finished deploying you'll see a link to the Azure Resource Group created for the solution.
1. To make any changes to the app code, just run:

    ```shell
    azd deploy
    ```

## Scaling the Proxy Service

The proxy service is stateless and scales vertically and horizontally. By default, the proxy service is configured to scale up to 10 replicas. The proxy service is configured to scale up to 10 replicas. The number of replicas can be changed from the Azure Portal or from the az cli. For example, to scale to 30 replicas using the az cli, change the:

```shell
az containerapp update -n $APP_NAME -g $RESOURCE_GROUP --subscription $SUBSCRIPTION_ID --replica 30
```

## Managing resources

You can configure multiple resources with the AI Proxy.

| Model deployment class | Resource | Description |
| ---------------------- | ------ | ----------- |
| `openai-chat` | gpt-35-turbo, gpt-35-turbo-16k, or newer | This is the model deployment class for the Azure OpenAI Chat Completions API. |
| `openai-completions` | davinci-002 or newer | This is the model deployment class for the Azure OpenAI Completions API. |
| `openai-embeddings` | text-embedding-ada-002 or newer | This is the model deployment class for the Azure OpenAI Embeddings API. |
| `azure-ai-search` | Azure AI Search index name | This allows for pass through acess to an instance of Azure AI Search |
| `openai-dall-e-3` | dall-3-e | This is the model deployment class for the Azure OpenAI Dall-e-3 models. |
| `openai-dall-e-2` | No model is deploy, just an Azure OpenAI resource in a location that supports the Images Generations API | This is the model deployment class for the Azure OpenAI Images Generations API. |

## Load balancing models

You can deploy multiple models of the same model deployment class. For example, you can deploy multiple `gpt-35-turbo` models in difference Azure OpenAI resources with the same name. The proxy will round robin across the models of the same deployment name to balance the load.

## Deploy an Azure OpenAI models

1. Open the Azure Portal.
1. Create a Azure resource group for your models. Naming suggestions include `openai-proxy-models`.
1. Create an Azure OpenAI resource in your subscription and add them to your models resource group. See [Create and deploy an Azure OpenAI Service resource](https://learn.microsoft.com/azure/ai-services/openai/how-to/create-resource) for more information.
   - Make a note of the `endpoint_key` and `endpoint_url` as you'll need them for the next step.
     - You can find the `endpoint_key` and `endpoint_url` in the Azure Portal under the `Keys and Endpoint` tab for the Azure OpenAI resource.
1. Create an Azure OpenAI model deployment. See [Create an Azure OpenAI model deployment](https://learn.microsoft.com/azure/ai-services/openai/how-to/create-resource?pivots=web-portal#deploy-a-model) for more information. From the Azure Portal, select the Azure OpenAI resource, then select the `Deployments` tab, and finally select `Create deployment`

   1. Select the `+ Create new deployment`.
   2. Select the `Model`.
   3. `Name` the deployment. Make a note of the name as you'll need it for the next step.
   4. Select `Advanced options`, and select the `Tokens per Minute Rate Limit`.
   5. Select `Create`.
