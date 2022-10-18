namespace Blog.Services.Identity.Domain.Exceptions;

public class EmailConfirmationCodeExpirationException : IdentityDomainException
{
    public EmailConfirmationCodeExpirationException(string message)
        : base(message)
    { }
}