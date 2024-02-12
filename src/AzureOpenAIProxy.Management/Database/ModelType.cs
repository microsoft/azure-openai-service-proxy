using NpgsqlTypes;

namespace AzureOpenAIProxy.Management.Database;

public enum ModelType
{
    [PgName("openai-chat")]
    OpenAI_Chat,
    [PgName("openai-embedding")]
    OpenAI_Embedding,
    [PgName("openai-dalle2")]
    OpenAI_Dalle2,
    [PgName("openai-dalle3")]
    OpenAI_Dalle3,
    [PgName("openai-whisper")]
    OpenAI_Whisper,
    [PgName("openai-completion")]
    OpenAI_Completion,
    [PgName("azure-ai-search")]
    Azure_AI_Search
}
