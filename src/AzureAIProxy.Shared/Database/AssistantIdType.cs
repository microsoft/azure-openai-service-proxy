using NpgsqlTypes;

namespace AzureAIProxy.Shared.Database;

public enum AssistantIdType
{
    [PgName("openai-assistant")]
    OpenAI_Assistant,
    [PgName("openai-file")]
    OpenAI_File,
    [PgName("openai-thread")]
    OpenAI_Thread
}

public static class AssistantIdTypeExtensions
{
    public static AssistantIdType ParsePostgresValue(string value)
    {
        return value switch
        {
            "openai-assistant" => AssistantIdType.OpenAI_Assistant,
            "openai-file" => AssistantIdType.OpenAI_File,
            "openai-thread" => AssistantIdType.OpenAI_Thread,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}
