// Chat completions your data example

// https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/openai/Azure.AI.OpenAI/tests/Samples/Sample08_UseYourOwnData.cs
// # Create a new Azure Cognitive Search index and load an index with Azure content
// # https://microsoftlearning.github.io/mslearn-knowledge-mining/Instructions/Labs/10-vector-search-exercise.html

using Azure.AI.OpenAI;
using DotNetEnv;

Env.Load();

// Get the key from the environment variables
string? key = Environment.GetEnvironmentVariable("YOUR_EVENT_AUTH_TOKEN");
string? endpoint = Environment.GetEnvironmentVariable("YOUR_AZURE_OPENAI_PROXY_URL");

string? searchEndpoint = Environment.GetEnvironmentVariable("YOUR_AZURE_SEARCH_ENDPOINT");
string? indexName = Environment.GetEnvironmentVariable("YOUR_AZURE_SEARCH_INDEX_NAME");
string? searchKey = Environment.GetEnvironmentVariable("YOUR_AZURE_SEARCH_KEY");

if (key == null || endpoint == null || searchEndpoint == null || indexName == null || searchKey == null)
{
    Console.WriteLine("Please set the YOUR_EVENT_AUTH_TOKEN, YOUR_AZURE_OPENAI_PROXY_URL, YOUR_AZURE_SEARCH_ENDPOINT, YOUR_AZURE_SEARCH_INDEX_NAME, and YOUR_AZURE_SEARCH_KEY environment variables.");
    return;
}

endpoint += "/api/v1";

var client = new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));

AzureCognitiveSearchChatExtensionConfiguration contosoExtensionConfig = new()
{
    SearchEndpoint = new Uri(searchEndpoint),
    IndexName = indexName,
};

contosoExtensionConfig.SetSearchKey(searchKey);



var chatCompletionsOptions = new ChatCompletionsOptions()
{
    DeploymentName = "gpt-3.5-turbo",
    Messages =
    {
        new ChatMessage(ChatRole.User, "What are the differences between Azure Machine Learning and Azure AI services?"),

    },
    AzureExtensionsOptions = new AzureChatExtensionsOptions()
    {
        Extensions = { contosoExtensionConfig }
    }
};

Azure.Response<ChatCompletions> completionsResponse = client.GetChatCompletions(chatCompletionsOptions);

var completion = completionsResponse.Value.Choices[0].Message.Content;
Console.WriteLine($"Content: {completion}");

var role = completionsResponse.Value.Choices[0].Message.AzureExtensionsContext.Messages[0].Role;
Console.WriteLine($"\nrole: {role}");

var context = completionsResponse.Value.Choices[0].Message.AzureExtensionsContext.Messages[0].Content;
Console.WriteLine($"\nContext: {context}");
