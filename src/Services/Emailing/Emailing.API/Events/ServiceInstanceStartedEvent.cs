namespace Blog.Services.Emailing.API.Events;

public record ServiceInstanceStartedEvent(string ServiceType, IEnumerable<string> ServiceBaseUrls);