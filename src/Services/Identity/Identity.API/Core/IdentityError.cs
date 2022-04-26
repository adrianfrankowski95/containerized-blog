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

    public static IdentityError InvalidIdentifier
        => new(IdentityErrorCode.InvalidCredentials, "Invalid identifier");

    public static IdentityError AccountLocked
        => new(IdentityErrorCode.AccountLocked, "Too many login attempts, please try again later");

    public static IdentityError AccountSuspended
        => new(IdentityErrorCode.AccountSuspended, "Account suspended");

    public static IdentityError ResettingPassword
        => new(IdentityErrorCode.ResettingPassword, "Inactive password");

    public static IdentityError MissingEmail
        => new(IdentityErrorCode.MissingEmail, "Missing email");

    public static IdentityError InvalidEmailFormat
        => new(IdentityErrorCode.InvalidEmailFormat, "Invalid email format");

    public static IdentityError EmailDuplicated
        => new(IdentityErrorCode.EmailDuplicated, "This email already exists");

    public static IdentityError EmailUnconfirmed
    => new(IdentityErrorCode.EmailUnconfirmed, "Unconfirmed email");

    public static IdentityError MissingUsername
        => new(IdentityErrorCode.MissingUsername, "Missing username");

    public static IdentityError InvalidUsernameFormat
        => new(IdentityErrorCode.InvalidUsernameFormat, "Invalid username format");

    public static IdentityError UsernameDuplicated
        => new(IdentityErrorCode.UsernameDuplicated, "This username already exists");

    public static IdentityError MissingPassword
        => new(IdentityErrorCode.MissingPassword, "Missing password");

    public static IdentityError InvalidPassword
        => new(IdentityErrorCode.InvalidPassword, "Invalid password");

    public static IdentityError InvalidPasswordFormat
        => new(IdentityErrorCode.InvalidPasswordFormat, "Invalid password format");

    public static IdentityError PasswordOkNeedsRehash
        => new(IdentityErrorCode.PasswordOkNeedsRehash, "Password needs rehash");

    public static IdentityError MissingSecurityStamp
        => new(IdentityErrorCode.MissingSecurityStamp, "Missing security stamp");

    public static IdentityError InvalidSecurityStamp
        => new(IdentityErrorCode.InvalidSecurityStamp, "Invalid security stamp");
}
