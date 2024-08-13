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
    /// <param name="apiKey">The API key associated with the assistant.</param>
    /// <param name="responseContent">The response content containing the assistant ID.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddIdAsync(string apiKey, string responseContent)
    {
        var response = JsonSerializer.Deserialize<AssistantResponse>(responseContent);
        var id = response?.Id;

        if (id is null) return;

        var assistant = new Assistant
        {
            ApiKey = apiKey,
            Id = id
        };

        db.Assistants.Add(assistant);
        await db.SaveChangesAsync();

        memoryCache.Set($"assistant-id-key+{id}+{apiKey}", assistant, TimeSpan.FromMinutes(10));
    }

    /// <summary>
    /// Deletes an assistant record from the database based on the provided API key and response content.
    /// </summary>
    /// <param name="apiKey">The API key associated with the assistant.</param>
    /// <param name="responseContent">The response content containing the assistant ID and deletion status.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteIdAsync(string apiKey, string responseContent)
    {
        var response = JsonSerializer.Deserialize<AssistantResponse>(responseContent);
        var id = response?.Id;
        var deleted = response?.Deleted ?? false;

        if (id is null || !deleted) return;

        var assistant = await db.Assistants
            .FirstOrDefaultAsync(a => a.ApiKey == apiKey && a.Id == id);

        if (assistant != null)
        {
            db.Assistants.Remove(assistant);
            await db.SaveChangesAsync();

            if (memoryCache.TryGetValue($"assistant-id-key+{id}+{apiKey}", out _))
                memoryCache.Remove($"assistant-id-key+{id}+{apiKey}");
        }
    }

    /// <summary>
    /// Retrieves a list of assistant records from the database based on the provided API key and assistant ID.
    /// </summary>
    /// <param name="apiKey">The API key associated with the assistant.</param>
    /// <param name="id">The assistant ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of assistant records.</returns>
    public async Task<Assistant?> GetIdAsync(string apiKey, string id)
    {
        if (memoryCache.TryGetValue($"assistant-id-key+{id}+{apiKey}", out Assistant? cachedId))
            return cachedId!;

        var result = await db.Assistants
            .FirstOrDefaultAsync(a => a.ApiKey == apiKey && a.Id == id);

        if (result != null)
            memoryCache.Set($"assistant-id-key+{id}+{apiKey}", result, TimeSpan.FromMinutes(10));

        return result;
    }

    /// <summary>
    /// Retrieves a list of assistant records from the database based on the provided assistant ID.
    /// This method is used to determine if an assistant asset has an owner.
    /// </summary>
    /// <param name="id">The assistant ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of assistant records.</returns>
    public Task<Assistant?> GetIdAsync(string id)
    {
        return db.Assistants
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();
    }
}