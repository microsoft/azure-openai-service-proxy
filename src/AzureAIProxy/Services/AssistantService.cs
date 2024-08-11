using AzureAIProxy.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Text.Json;

namespace AzureAIProxy.Services;

public class AssistantService(AzureAIProxyDbContext db) : IAssistantService
{
    public async Task AddIdAsync(string api_key, string responseContent, AssistantType idType)
    {
        var jsonElement = JsonDocument.Parse(responseContent).RootElement;
        var id = jsonElement.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
        if (id is null) return;

        var assistant = new Assistant
        {
            ApiKey = api_key,
            Id = id,
            Type = idType
        };

        db.Assistants.Add(assistant);
        await db.SaveChangesAsync();
    }

    public async Task DeleteIdAsync(string api_key, string responseContent, AssistantType idType)
    {
        var jsonElement = JsonDocument.Parse(responseContent).RootElement;
        var id = jsonElement.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
        var deleted = jsonElement.TryGetProperty("deleted", out var deletedElement) && deletedElement.GetBoolean();
        if (id is null || !deleted) return;

        var assistant = await db.Assistants
            .Where(a => a.ApiKey == api_key && a.Id == id && a.Type == idType)
            .FirstOrDefaultAsync();

        if (assistant != null)
        {
            db.Assistants.Remove(assistant);
            await db.SaveChangesAsync();
        }
    }

    public Task<List<Assistant>> GetIdsAsync(string api_key, AssistantType idType)
    {
        return db.Assistants
            .Where(a => a.ApiKey == api_key && a.Type == idType)
            .ToListAsync();
    }
}
