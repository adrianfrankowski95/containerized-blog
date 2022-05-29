namespace Blog.Services.Identity.API.Core;

public class EmailValidationError : IdentityError
{
    private EmailValidationError(IdentityErrorCode errorCode, string errorDescription)
        : base(errorCode, errorDescription) { }

    public static EmailValidationError MissingEmail
        => new(IdentityErrorCode.MissingEmail, "The Email is required.");

    public static EmailValidationError InvalidEmailFormat
        => new(IdentityErrorCode.InvalidEmailFormat, "The Email has invalid format.");

    public static EmailValidationError EmailDuplicated
        => new(IdentityErrorCode.EmailDuplicated, "The Email is already in use.");

    public static EmailValidationError EmailUnconfirmed
        => new(IdentityErrorCode.EmailUnconfirmed, "The Email is unconfirmed.");

}