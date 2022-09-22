//namespace of the event must be the same in Producers and in Consumers
//in order to make it work through the MassTransit
namespace Blog.Integration.Events;

public record ServiceInstanceStoppedIntegrationEvent(Guid InstanceId, string ServiceType, HashSet<string> ServiceAddresses);