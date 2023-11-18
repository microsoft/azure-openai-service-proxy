// Replace with your Azure OpenAI key
using Azure.AI.OpenAI;
using DotNetEnv;

internal class Program
{
    private static void Main(string[] args)
    {
        Env.Load();

        // Get the key from the environment variables
        string key = Environment.GetEnvironmentVariable("YOUR_EVENT_AUTH_TOKEN");
        string endpoint = Environment.GetEnvironmentVariable("YOUR_AZURE_OPENAI_PROXY_URL") + "/v1/api";

        var client = new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));

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
    }
}