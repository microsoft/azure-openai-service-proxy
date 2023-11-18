// Chat completions example
using Azure.AI.OpenAI;

string key = "YOUR_EVENT_CODE/YOUR_GITHUB_USERNAME";
string endpoint = "https://YOUR_AZURE_OPENAI_PROXY_URL/v1/api";
var client = new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));


var chatCompletionsOptions = new ChatCompletionsOptions()
{
    DeploymentName = "gpt-3.5-turbo",
    Messages =
    {
        new ChatMessage(ChatRole.System, "You are a helpful assistant. You will talk like a pirate."),
        new ChatMessage(ChatRole.User, "Can you help me?"),
        new ChatMessage(ChatRole.Assistant, "Arrrr! Of course, me hearty! What can I do for ye?"),
        new ChatMessage(ChatRole.User, "What's the best way to train a parrot?"),
    }
};

Azure.Response<ChatCompletions> completionsResponse = client.GetChatCompletions(chatCompletionsOptions);

var completion = completionsResponse.Value.Choices[0].Message.Content;
Console.WriteLine(completion);
