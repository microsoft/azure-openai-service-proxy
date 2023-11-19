using System.Text;
using DotNetEnv;

class Program
{
    static async Task Main(string[] args)
    {

        Env.Load();

        // Get the key from the environment variables
        string api_key = Environment.GetEnvironmentVariable("YOUR_EVENT_AUTH_TOKEN");
        string endpoint = Environment.GetEnvironmentVariable("YOUR_AZURE_OPENAI_PROXY_URL") + "/v1/api/openai/deployments/dalle3/images/generations";

        var client = new HttpClient();

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(endpoint),
        };

        request.Headers.Add("api-key", api_key); // replace with your API key

        var jsonObject = new { prompt = "cute picture of an cat", n = 1 }; // replace with your JSON object
        var json = System.Text.Json.JsonSerializer.Serialize(jsonObject);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);

        var responseContent = await response.Content.ReadAsStringAsync();

        Console.WriteLine(responseContent);
    }
}