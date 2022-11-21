using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.Events;

public record PasswordChangedDomainEvent : DomainEvent
{
    public Username Username { get; }
    public EmailAddress EmailAddress { get; }
    public PasswordChangedDomainEvent(Username username, EmailAddress emailAddress)
    {
        Username = username;
        EmailAddress = emailAddress;
    }
}