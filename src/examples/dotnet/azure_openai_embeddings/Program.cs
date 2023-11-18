using Azure.AI.OpenAI;
using DotNetEnv;

internal class Program
{
    private static async global::System.Threading.Tasks.Task Main(string[] args)
    {
        Env.Load();

        // Get the key from the environment variables
        string key = Environment.GetEnvironmentVariable("YOUR_EVENT_AUTH_TOKEN");
        string endpoint = Environment.GetEnvironmentVariable("YOUR_AZURE_OPENAI_PROXY_URL") + "/v1/api";

        var client = new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));

        EmbeddingsOptions embeddingsOptions = new()
        {
            DeploymentName = "text-embedding-ada-002",
            Input = { "Your text string goes here" },
        };
        Azure.Response<Embeddings> response = await client.GetEmbeddingsAsync(embeddingsOptions);


        EmbeddingItem item = response.Value.Data[0];
        ReadOnlyMemory<float> embedding = item.Embedding;
        Console.WriteLine($"Embedding vector length: {embedding.Length}");
        Console.WriteLine($"Embedding vector: {string.Join(", ", embedding.ToArray())}");
    }
}