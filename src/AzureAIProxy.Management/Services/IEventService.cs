using AzureAIProxy.Management.Components.EventManagement;

namespace AzureAIProxy.Management.Services;

public interface IEventService
{
    Task<IEnumerable<Event>> GetOwnerEventsAsync();

    Task<Event?> GetEventAsync(string id);

    Task<Event?> CreateEventAsync(EventEditorModel model);

    Task<Event?> UpdateEventAsync(string id, EventEditorModel model);

    Task UpdateModelsForEventAsync(string id, IEnumerable<Guid> modelIds);

    Task DeleteEventAsync(string id);
}
