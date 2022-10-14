using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.Events;

public record UserCreatedDomainEvent : DomainEvent
{
    public Username Username { get; }
    public EmailAddress EmailAddress { get; }
    public PasswordResetCode PasswordResetCode { get; }
    public UserCreatedDomainEvent(Username username, EmailAddress emailAddress, PasswordResetCode passwordResetCode)
    {
        if (username is null)
            throw new ArgumentNullException(nameof(username));

        if (emailAddress is null)
            throw new ArgumentNullException(nameof(emailAddress));

        if (passwordResetCode is null || passwordResetCode.IsEmpty)
            throw new ArgumentNullException(nameof(passwordResetCode));

        Username = username;
        EmailAddress = emailAddress;
        PasswordResetCode = passwordResetCode;
    }
}