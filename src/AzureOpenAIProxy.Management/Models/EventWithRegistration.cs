namespace AzureOpenAIProxy.Management.Models;

public record EventWithRegistration(string Name, string OrganizerName, DateTime StartDate, DateTime EndDate, int RegistrationCount, string EventId);
