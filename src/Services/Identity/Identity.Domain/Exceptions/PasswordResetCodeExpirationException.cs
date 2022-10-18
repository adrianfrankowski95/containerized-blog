namespace Blog.Services.Identity.Domain.Exceptions;

public class PasswordResetCodeExpirationException : IdentityDomainException
{
    public PasswordResetCodeExpirationException(string message)
        : base(message)
    { }
}