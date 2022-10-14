namespace Blog.Services.Identity.Domain.Exceptions;

public class EmailConfirmationCodeExpiredException : IdentityDomainException
{
    public EmailConfirmationCodeExpiredException(string message)
        : base(message)
    { }
}