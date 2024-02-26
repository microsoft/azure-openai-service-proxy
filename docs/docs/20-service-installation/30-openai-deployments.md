# Managing models

## Understanding Azure OpenAI model deployments

As at November 2023, the proxy supports the following model deployment classes:

| Model deployment class | Models | Description |
| ---------------------- | ------ | ----------- |
| `openai-chat` | gpt-35-turbo, gpt-35-turbo-16k, or newer | This is the model deployment class for the Azure OpenAI Chat Completions API. |
| `openai-completions` | davinci-002 or newer | This is the model deployment class for the Azure OpenAI Completions API. |
| `openai-embeddings` | text-embedding-ada-002 or newer | This is the model deployment class for the Azure OpenAI Embeddings API. |
| `openai-images-generations` | No model is deploy, just an Azure OpenAI resource in a location that supports the Images Generations API | This is the model deployment class for the Azure OpenAI Images Generations API. |
| `azure-ai-search` | Not applicable | This allows for pass through acess to an instance of Azure AI Search |


:::tip

You can deploy multiple models of the same model deployment class. For example, you can deploy multiple `gpt-35-turbo` models in difference Azure OpenAI resources with the same name. The proxy will round robin across the models of the same deployment name to balance the load.

:::

## Deploy an Azure OpenAI models

1. Open the Azure Portal.
2. Create an Azure OpenAI resource in your subscription. See [Create and deploy an Azure OpenAI Service resource](https://learn.microsoft.com/azure/ai-services/openai/how-to/create-resource) for more information.
   - Make a note of the `endpoint_key` and `endpoint_url` as you'll need them for the next step.
     - You can find the `endpoint_key` and `endpoint_url` in the Azure Portal under the `Keys and Endpoint` tab for the Azure OpenAI resource.
3. Create an Azure OpenAI model deployment. See [Create an Azure OpenAI model deployment](https://learn.microsoft.com/azure/ai-services/openai/how-to/create-resource?pivots=web-portal#deploy-a-model) for more information. From the Azure Portal, select the Azure OpenAI resource, then select the `Deployments` tab, and finally select `Create deployment`

   1. Select the `+ Create new deployment`.
   2. Select the `Model`.
   3. `Name` the deployment. Make a note of the name as you'll need it for the next step.
   4. Select `Advanced options`, and select the `Tokens per Minute Rate Limit`.
   5. Select `Create`.
