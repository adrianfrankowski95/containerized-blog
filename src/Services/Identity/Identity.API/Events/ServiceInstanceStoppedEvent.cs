namespace Blog.Services.Messaging.Events;

//namespace of the event must be the same in Producers and in Consumers
//in order to make it work through the MassTransit
public record ServiceInstanceStoppedEvent(string ServiceType, IEnumerable<string> ServiceBaseUrls);