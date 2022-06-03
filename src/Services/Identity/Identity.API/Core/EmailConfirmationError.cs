namespace Blog.Services.Identity.API.Core;

public class EmailConfirmationError : IdentityError
{
    private EmailConfirmationError(IdentityErrorCode errorCode, string errorDescription)
        : base(errorCode, errorDescription) { }

    public static EmailConfirmationError InvalidEmailConfirmationCode
        => new(IdentityErrorCode.InvalidEmailConfirmationCode, "The Email address confirmation code is invalid.");

    public static EmailConfirmationError ExpiredEmailConfirmationCode
         => new(IdentityErrorCode.ExpiredEmailConfirmationCode, "The Email address confirmation code has expired.");
}