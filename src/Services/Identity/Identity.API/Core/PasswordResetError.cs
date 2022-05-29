namespace Blog.Services.Identity.API.Core;

public class PasswordResetError : IdentityError
{
    private PasswordResetError(IdentityErrorCode errorCode, string errorDescription)
        : base(errorCode, errorDescription) { }

    public static PasswordResetError InvalidPasswordResetCode
    => new(IdentityErrorCode.InvalidPasswordResetCode, "The Password reset code is invalid.");

    public static PasswordResetError ExpiredPasswordResetCode
         => new(IdentityErrorCode.ExpiredPasswordResetCode, "The Password reset code has expired.");

    public static PasswordResetError PasswordResetNotRequested
        => new(IdentityErrorCode.PasswordResetNotRequested, "The Password reset has not been requested.");
}