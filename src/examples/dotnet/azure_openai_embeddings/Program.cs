using Azure.AI.OpenAI;

string key = "YOUR_EVENT_CODE/YOUR_GITHUB_USERNAME";
string endpoint = "https://YOUR_AZURE_OPENAI_PROXY_URL/v1/api";
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