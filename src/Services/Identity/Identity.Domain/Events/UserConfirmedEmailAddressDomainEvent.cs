using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.Events;

public record UserConfirmedEmailAddressDomainEvent : DomainEvent
{
    public Username Username { get; }
    public EmailAddress EmailAddress { get; }
    public UserConfirmedEmailAddressDomainEvent(Username username, EmailAddress emailAddress)
    {
        if (username is null)
            throw new ArgumentNullException(nameof(username));

        if (emailAddress is null)
            throw new ArgumentNullException(nameof(emailAddress));

        Username = username;
        EmailAddress = emailAddress;
    }
}