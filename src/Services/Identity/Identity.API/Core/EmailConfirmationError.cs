namespace Blog.Services.Identity.API.Core;

public class EmailConfirmationError : IdentityError
{
    private EmailConfirmationError(IdentityErrorCode errorCode, string errorDescription)
        : base(errorCode, errorDescription) { }

    public static EmailConfirmationError EmailAlreadyConfirmed
    => new(IdentityErrorCode.EmailAlreadyConfirmed, "The Email is already confirmed.");

    public static EmailConfirmationError InvalidEmailConfirmationCode
        => new(IdentityErrorCode.InvalidEmailConfirmationCode, "The Email confirmation code is invalid.");

    public static EmailConfirmationError ExpiredEmailConfirmationCode
         => new(IdentityErrorCode.ExpiredEmailConfirmationCode, "The Email confirmation code has expired.");
}