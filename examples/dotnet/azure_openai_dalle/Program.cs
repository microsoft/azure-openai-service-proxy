using System.Text;
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

await GenerateWithDalle3(key, endpoint);

static async Task GenerateWithDalle3(string key, string endpoint)
{
    endpoint += "/openai/deployments/dall-e-3/images/generations";

    Console.WriteLine("Generating with DALL-E v3");
    var client = new HttpClient();

    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri(endpoint),
    };

    request.Headers.Add("api-key", key);

    var jsonObject = new { prompt = "cute picture of an cat", n = 1 };
    var json = System.Text.Json.JsonSerializer.Serialize(jsonObject);
    request.Content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.SendAsync(request);

    var responseContent = await response.Content.ReadAsStringAsync();

    Console.WriteLine(responseContent);
}
