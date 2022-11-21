using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.Events;

public record UserRegisteredDomainEvent : DomainEvent
{
    public Username Username { get; }
    public EmailAddress EmailAddress { get; }
    public EmailConfirmationCode EmailConfirmationCode { get; }
    public UserRegisteredDomainEvent(Username username, EmailAddress emailAddress, EmailConfirmationCode emailConfirmationCode)
    {
        if (emailConfirmationCode.IsEmpty)
            throw new ArgumentNullException(nameof(emailConfirmationCode));

        Username = username;
        EmailAddress = emailAddress;
        EmailConfirmationCode = emailConfirmationCode;
    }
}