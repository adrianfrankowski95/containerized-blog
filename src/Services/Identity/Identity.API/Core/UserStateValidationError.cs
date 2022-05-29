namespace Blog.Services.Identity.API.Core;

public class UserStateValidationError : IdentityError
{
    private UserStateValidationError(IdentityErrorCode errorCode, string errorDescription)
        : base(errorCode, errorDescription) { }

    public static UserStateValidationError AccountLockedOut
    => new(IdentityErrorCode.AccountLockedOut, "This account has been temporarily locked out.");

    public static UserStateValidationError AccountSuspended
        => new(IdentityErrorCode.AccountSuspended, "This account has been suspended.");

    public static UserStateValidationError ResettingPassword
        => new(IdentityErrorCode.ResettingPassword, "This account has requested a password reset.");
}