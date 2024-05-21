using AzureOpenAIProxy.Management.Components.EventManagement;
using AzureOpenAIProxy.Management.Database;

namespace AzureOpenAIProxy.Management.Services;

public interface IEventService
{
    Task<IEnumerable<Event>> GetOwnerEventsAsync();

    Task<Event?> GetEventAsync(string id);

    Task<Event?> CreateEventAsync(EventEditorModel model);

    Task<Event?> UpdateEventAsync(string id, EventEditorModel model);

    Task<Event?> UpdateModelsForEventAsync(string id, IEnumerable<Guid> modelIds);

    Task DeleteEventAsync(string id);
}
