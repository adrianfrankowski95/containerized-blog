using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.Events;

public record EmailAddressConfirmedDomainEvent : DomainEvent
{
    public Username Username { get; }
    public EmailAddress EmailAddress { get; }
    public EmailAddressConfirmedDomainEvent(Username username, EmailAddress emailAddress)
    {
        if (username is null)
            throw new ArgumentNullException(nameof(username));

        if (emailAddress is null)
            throw new ArgumentNullException(nameof(emailAddress));

        Username = username;
        EmailAddress = emailAddress;
    }
}