using NpgsqlTypes;

namespace AzureOpenAIProxy.Management.Database;

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
    Azure_AI_Search
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
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    public static string ToPostgresValue(this ModelType modelType)
    {
        return modelType switch
        {
            ModelType.OpenAI_Chat => "openai-chat",
            ModelType.OpenAI_Embedding => "openai-embedding",
            ModelType.OpenAI_Dalle3 => "openai-dalle3",
            ModelType.OpenAI_Whisper => "openai-whisper",
            ModelType.OpenAI_Completion => "openai-completion",
            ModelType.Azure_AI_Search => "azure-ai-search",
            _ => throw new ArgumentOutOfRangeException(nameof(modelType), modelType, null)
        };
    }
}
