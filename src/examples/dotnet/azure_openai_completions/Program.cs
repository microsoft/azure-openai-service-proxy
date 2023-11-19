// Replace with your Azure OpenAI key
using Azure.AI.OpenAI;
using DotNetEnv;

Env.Load();

// Get the key from the environment variables
string? key = Environment.GetEnvironmentVariable("YOUR_EVENT_AUTH_TOKEN");
string? endpoint = Environment.GetEnvironmentVariable("YOUR_AZURE_OPENAI_PROXY_URL");

if (key == null || endpoint == null)
{
    Console.WriteLine("Please set the YOUR_EVENT_AUTH_TOKEN and YOUR_AZURE_OPENAI_PROXY_URL environment variables.");
    return;
}

var client = new OpenAIClient(new Uri(endpoint + "/v1/api"), new Azure.AzureKeyCredential(key));

CompletionsOptions completionsOptions = new()
{
    DeploymentName = "text-davinci-003",
    Prompts =
    {
        "How are you today?",
        "What is Azure OpenAI?",
        "Why do children love dinosaurs?",
        "Generate a proof of Euler's identity",
        "Describe in single words only the good things that come into your mind about your mother."
    },
};

Azure.Response<Completions> completionsResponse = client.GetCompletions(completionsOptions);

foreach (Choice choice in completionsResponse.Value.Choices)
{
    Console.WriteLine($"Response for prompt {choice.Index}: {choice.Text}");
}
