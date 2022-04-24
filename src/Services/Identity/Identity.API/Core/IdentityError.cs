namespace Blog.Services.Identity.API.Core;

public class IdentityError
{
    public IdentityErrorCode ErrorCode { get; }
    public string ErrorDescription { get; }

    private IdentityError(IdentityErrorCode errorCode, string errorDescription)
    {
        ErrorCode = errorCode;
        ErrorDescription = errorDescription;
    }

    public static IdentityError InvalidCredentials
        => new(IdentityErrorCode.InvalidCredentials, "Invalid Email and/or Password");

    public static IdentityError AccountLocked
        => new(IdentityErrorCode.AccountLocked, "Too many login attempts, please try again later");

    public static IdentityError AccountSuspended
        => new(IdentityErrorCode.AccountSuspended, "Account suspended");

    public static IdentityError UnconfirmedEmail
        => new(IdentityErrorCode.UnconfirmedEmail, "Unconfirmed email");

    public static IdentityError ResettingPassword
        => new(IdentityErrorCode.ResettingPassword, "Inactive password");
}
