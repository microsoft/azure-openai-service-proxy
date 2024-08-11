using NpgsqlTypes;

namespace AzureAIProxy.Shared.Database;

public enum AssistantType
{
    [PgName("assistant")]
    OpenAI_Assistant,
    [PgName("file")]
    OpenAI_File,
    [PgName("thread")]
    OpenAI_Thread
}

public static class TypeExtensions
{
    public static AssistantType ParsePostgresValue(string value)
    {
        return value switch
        {
            "assistant" => AssistantType.OpenAI_Assistant,
            "file" => AssistantType.OpenAI_File,
            "thread" => AssistantType.OpenAI_Thread,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}
