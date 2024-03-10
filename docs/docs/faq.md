# Frequently asked questions

1. I've create an event but no models are available to select in the Playground. What's wrong?
    Create an OpenAI Chat Completion models in Azure. From the Azure AI Proxy Admin portal, select the `Resources` tab and create a new resource of type `OpenAI-Chat` using the Azure OpenAI resource key, endpoint and deployment name. Once the resource is created, edit the event to add the resource.
