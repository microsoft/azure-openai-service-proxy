using AzureAIProxy.Shared.Database;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AzureAIProxy.Services;

public class AssistantService(AzureAIProxyDbContext db) : IAssistantService
{
    public async Task AddIdAsync(string api_key, string responseContent, AssistantIdType idType)
    {
        var id = JsonDocument.Parse(responseContent).RootElement.GetProperty("id").GetString();
        if (id is null) return;

        var assistant = new Assistant
        {
            ApiKey = api_key,
            Id = id,
            IdType = idType
        };

        db.Assistants.Add(assistant);
        await db.SaveChangesAsync();
    }

    public async Task DeleteIdAsync(string api_key, string responseContent, AssistantIdType idType)
    {
        var id = JsonDocument.Parse(responseContent).RootElement.GetProperty("id").GetString();
        if (id is null) return;

        var assistant = await db.Assistants
            .Where(a => a.ApiKey == api_key && a.Id == id && a.IdType == idType)
            .FirstOrDefaultAsync();

        if (assistant != null)
        {
            db.Assistants.Remove(assistant);
            await db.SaveChangesAsync();
        }
    }

    public Task<List<Assistant>> GetIdsAsync(string api_key, AssistantIdType idType)
    {
        return db.Assistants
            .Where(a => a.ApiKey == api_key && a.IdType == idType)
            .ToListAsync();
    }
}
