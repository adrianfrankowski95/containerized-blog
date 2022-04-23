namespace Blog.Services.Identity.API.Core;

public class IdentityError
{
    public IdentityErrorCode Code { get; }
    public string Description { get; }

    private IdentityError(IdentityErrorCode errorCode, string errorDescription)
    {
        Code = errorCode;
        Description = errorDescription;
    }

    public static IdentityError InvalidCredentials
        => new(IdentityErrorCode.InvalidCredentials, "Invalid Email and/or Password");

    public static IdentityError InvalidPassword
        => new(IdentityErrorCode.InvalidPassword, "Invalid password");

    public static IdentityError AccountLocked
        => new(IdentityErrorCode.AccountLocked, "Too many login attempts, please try again later");

    public static IdentityError AccountSuspended
        => new(IdentityErrorCode.AccountSuspended, "Account suspended");

    public static IdentityError UnconfirmedEmail
        => new(IdentityErrorCode.UnconfirmedEmail, "Unconfirmed email");

    public static IdentityError ResettingPassword
        => new(IdentityErrorCode.ResettingPassword, "Inactive password");

    public static IdentityError InvalidOrMissingSecurityStamp
        => new(IdentityErrorCode.InvalidOrMissingSecurityStamp, "Invalid security stamp");

    public static IdentityError InvalidIdentifier
        => new(IdentityErrorCode.InvalidIdentifier, "Invalid Id");

    public static IdentityError InvalidUsername
        => new(IdentityErrorCode.InvalidUsername, "Invalid Username");

    public static IdentityError InvalidEmail
        => new(IdentityErrorCode.InvalidEmail, "Invalid Email");
}
