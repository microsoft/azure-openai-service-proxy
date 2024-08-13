using AzureAIProxy.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AzureAIProxy.Services;

/// <summary>
/// Provides services for managing assistant assets, including adding, deleting, and retrieving assistant records.
/// <param name="db">The database context for accessing assistant records.</param>
/// <param name="memoryCache">The memory cache for caching assistant records.</param>
/// </summary>
public class AssistantService(AzureAIProxyDbContext db, IMemoryCache memoryCache) : IAssistantService
{

    /// <summary>
    /// Adds a new assistant record to the database based on the provided API key and response content.
    /// </summary>
    /// <param name="api_key">The API key associated with the assistant.</param>
    /// <param name="responseContent">The response content containing the assistant ID.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

    /// <summary>
    /// Deletes an assistant record from the database based on the provided API key and response content.
    /// </summary>
    /// <param name="api_key">The API key associated with the assistant.</param>
    /// <param name="responseContent">The response content containing the assistant ID and deletion status.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

            if (memoryCache.TryGetValue(id + api_key, out _))
                memoryCache.Remove(id + api_key);
        }
    }

    /// <summary>
    /// Retrieves a list of assistant records from the database based on the provided API key and assistant ID.
    /// </summary>
    /// <param name="api_key">The API key associated with the assistant.</param>
    /// <param name="id">The assistant ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of assistant records.</returns>
    public async Task<List<Assistant>> GetIdAsync(string api_key, string id)
    {
        if (memoryCache.TryGetValue(id + api_key, out List<Assistant>? cachedId))
            return cachedId!;

        var result = await db.Assistants
            .Where(a => a.ApiKey == api_key && a.Id == id)
            .ToListAsync();

        memoryCache.Set(id + api_key, result, TimeSpan.FromMinutes(10));
        return result;
    }

    /// <summary>
    /// Retrieves a list of assistant records from the database based on the provided assistant ID.
    /// This method is used to determine if an assistant asset has an owner.
    /// </summary>
    /// <param name="id">The assistant ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of assistant records.</returns>
    public Task<List<Assistant>> GetIdAsync(string id)
    {
        return db.Assistants
            .Where(a => a.Id == id)
            .ToListAsync();
    }
}
