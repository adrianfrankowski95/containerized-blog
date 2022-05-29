namespace Blog.Services.Identity.API.Core;

public class PasswordValidationError : IdentityError
{
    private PasswordValidationError(IdentityErrorCode errorCode, string errorDescription)
        : base(errorCode, errorDescription) { }

    public static PasswordValidationError PasswordTooShort
        => new(IdentityErrorCode.PasswordTooShort, "The Password is too short.");

    public static PasswordValidationError PasswordWithoutLowerCase
        => new(IdentityErrorCode.PasswordWithoutLowerCase, "The Password must contain a lower-case character.");

    public static PasswordValidationError PasswordWithoutUpperCase
        => new(IdentityErrorCode.PasswordWithoutUpperCase, "The Password must contain an upper-case character.");

    public static PasswordValidationError PasswordWithoutDigit
        => new(IdentityErrorCode.PasswordWithoutDigit, "The Password must contain a digit.");

    public static PasswordValidationError PasswordWithoutNonAlphanumeric
        => new(IdentityErrorCode.PasswordWithoutNonAlphanumeric, "The Password must contain a non-alphanumeric character.");
}