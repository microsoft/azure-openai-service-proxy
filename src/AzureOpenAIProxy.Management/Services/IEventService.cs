using AzureOpenAIProxy.Management.Components.EventManagement;
using AzureOpenAIProxy.Management.Database;

namespace AzureOpenAIProxy.Management.Services;

public interface IEventService
{
    Task<IEnumerable<Event>> GetOwnerEventsAsync();

    Event? GetEvent(string id);

    Task<Event?> CreateEventAsync(EventEditorModel model);

    Event? UpdateEvent(string id, EventEditorModel model);

    Event? UpdateModelsForEvent(string id, IEnumerable<Guid> modelIds);
}
