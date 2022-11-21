using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.Events;

public record EmailAddressConfirmedDomainEvent : DomainEvent
{
    public Username Username { get; }
    public EmailAddress EmailAddress { get; }
    public EmailAddressConfirmedDomainEvent(Username username, EmailAddress emailAddress)
    {
        Username = username;
        EmailAddress = emailAddress;
    }
}