namespace Blog.Services.Identity.API.Core;

public class EmailValidationError : IdentityError
{
    private EmailValidationError(IdentityErrorCode errorCode, string errorDescription)
        : base(errorCode, errorDescription) { }

    public static EmailValidationError MissingEmail
        => new(IdentityErrorCode.MissingEmail, "The Email address is required.");

    public static EmailValidationError InvalidEmailFormat
        => new(IdentityErrorCode.InvalidEmailFormat, "The Email address has invalid format.");

    public static EmailValidationError EmailDuplicated
        => new(IdentityErrorCode.EmailDuplicated, "The Email address is already in use.");

    public static EmailValidationError NewAndOldEmailsAreEqual
        => new(IdentityErrorCode.NewAndOldEmailsAreEqual, "The New email and Old email address must be different.");
}