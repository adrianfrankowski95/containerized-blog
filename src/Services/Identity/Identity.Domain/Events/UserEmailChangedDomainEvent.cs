using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.Events;

public record UserEmailChangedDomainEvent : DomainEvent
{
    public Username Username { get; }
    public EmailAddress EmailAddress { get; }
    public EmailConfirmationCode EmailConfirmationCode { get; }
    public UserEmailChangedDomainEvent(Username username, EmailAddress emailAddress, EmailConfirmationCode emailConfirmationCode)
    {
        if (username is null)
            throw new ArgumentNullException(nameof(username));

        if (emailAddress is null)
            throw new ArgumentNullException(nameof(emailAddress));

        if (emailConfirmationCode is null || emailConfirmationCode.IsEmpty())
            throw new ArgumentNullException(nameof(emailConfirmationCode));

        Username = username;
        EmailAddress = emailAddress;
        EmailConfirmationCode = emailConfirmationCode;
    }
}