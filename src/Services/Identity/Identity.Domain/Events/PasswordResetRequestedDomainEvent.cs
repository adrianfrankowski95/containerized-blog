using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.Events;

public record PasswordResetRequestedDomainEvent : DomainEvent
{
    public Username Username { get; }
    public EmailAddress EmailAddress { get; }
    public PasswordResetCode PasswordResetCode { get; }
    public PasswordResetRequestedDomainEvent(Username username, EmailAddress emailAddress, PasswordResetCode passwordResetCode)
    {
        if (passwordResetCode.IsEmpty)
            throw new ArgumentNullException(nameof(passwordResetCode));

        Username = username;
        EmailAddress = emailAddress;
        PasswordResetCode = passwordResetCode;
    }
}