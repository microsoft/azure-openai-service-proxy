using AzureAIProxy.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AzureAIProxy.Services;

public class AssistantService(AzureAIProxyDbContext db, IMemoryCache memoryCache) : IAssistantService
{
    public async Task AddIdAsync(string api_key, string responseContent)
    {
        var jsonElement = JsonDocument.Parse(responseContent).RootElement;
        var id = jsonElement.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
        if (id is null) return;

        var assistant = new Assistant
        {
            ApiKey = api_key,
            Id = id
        };

        db.Assistants.Add(assistant);
        await db.SaveChangesAsync();
    }

    public async Task DeleteIdAsync(string api_key, string responseContent)
    {
        var jsonElement = JsonDocument.Parse(responseContent).RootElement;
        var id = jsonElement.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
        var deleted = jsonElement.TryGetProperty("deleted", out var deletedElement) && deletedElement.GetBoolean();
        if (id is null || !deleted) return;

        var assistant = await db.Assistants
            .Where(a => a.ApiKey == api_key && a.Id == id)
            .FirstOrDefaultAsync();

        if (assistant != null)
        {
            db.Assistants.Remove(assistant);
            await db.SaveChangesAsync();

            if (memoryCache.TryGetValue(api_key + id, out _))
                memoryCache.Remove(api_key + id);
        }
    }

    public async Task<List<Assistant>> GetIdAsync(string api_key, string id)
    {
        if (memoryCache.TryGetValue(api_key + id, out List<Assistant>? cachedId))
            return cachedId!;

        var result = await db.Assistants
            .Where(a => a.ApiKey == api_key && a.Id == id)
            .ToListAsync();

        memoryCache.Set(api_key + id, result, TimeSpan.FromMinutes(10));
        return result;
    }

    public Task<List<Assistant>> GetIdsAsync(string api_key, string type)
    {
        return db.Assistants
            .Where(a => a.ApiKey == api_key && a.Id.StartsWith(type))
            .ToListAsync();
    }
}
