//namespace of the event must be the same in Producers and in Consumers
//in order to make it work through the MassTransit
namespace Blog.Services.Integration.Events;

public record ServiceRegistryUpdatedEvent(IDictionary<string, IEnumerable<string>> UpdatedRegistry);