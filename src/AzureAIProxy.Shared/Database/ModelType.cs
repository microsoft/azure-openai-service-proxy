using NpgsqlTypes;

namespace AzureAIProxy.Shared.Database;

public enum ModelType
{
    [PgName("openai-chat")]
    OpenAI_Chat,
    [PgName("openai-embedding")]
    OpenAI_Embedding,
    [PgName("openai-dalle3")]
    OpenAI_Dalle3,
    [PgName("openai-whisper")]
    OpenAI_Whisper,
    [PgName("openai-completion")]
    OpenAI_Completion,
    [PgName("azure-ai-search")]
    Azure_AI_Search,
    [PgName("openai-assistant")]
    OpenAI_Assistant
}

public static class ModelTypeExtensions
{
    public static ModelType ParsePostgresValue(string value)
    {
        return value switch
        {
            "openai-chat" => ModelType.OpenAI_Chat,
            "openai-embedding" => ModelType.OpenAI_Embedding,
            "openai-dalle3" => ModelType.OpenAI_Dalle3,
            "openai-whisper" => ModelType.OpenAI_Whisper,
            "openai-completion" => ModelType.OpenAI_Completion,
            "azure-ai-search" => ModelType.Azure_AI_Search,
            "openai-assistant" => ModelType.OpenAI_Assistant,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}
