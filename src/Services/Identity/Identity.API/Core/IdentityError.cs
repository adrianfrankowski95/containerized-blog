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
        => new(IdentityErrorCode.AccountLocked, "Too many failed login attempts");

    public static IdentityError AccountSuspended
        => new(IdentityErrorCode.AccountSuspended, "Account suspended");

    public static IdentityError ResettingPassword
        => new(IdentityErrorCode.ResettingPassword, "Inactive password");

    public static IdentityError MissingEmail
        => new(IdentityErrorCode.MissingEmail, "Missing email");

    public static IdentityError InvalidEmailFormat
        => new(IdentityErrorCode.InvalidEmailFormat, "Invalid email format");

    public static IdentityError EmailDuplicated
        => new(IdentityErrorCode.EmailDuplicated, "This email address already exists");

    public static IdentityError EmailUnconfirmed
        => new(IdentityErrorCode.EmailUnconfirmed, "Unconfirmed email address");

    public static IdentityError EmailAlreadyConfirmed
        => new(IdentityErrorCode.EmailAlreadyConfirmed, "Email address is already confirmed");

    public static IdentityError InvalidEmailConfirmationCode
        => new(IdentityErrorCode.InvalidEmailConfirmationCode, "Invalid email address confirmation code");

    public static IdentityError MissingEmailConfirmationCode
        => new(IdentityErrorCode.MissingEmailConfirmationCode, "Missing email address confirmation code");

    public static IdentityError ExpiredEmailConfirmationCode
         => new(IdentityErrorCode.ExpiredEmailConfirmationCode, "Expired email address confirmation code");

    public static IdentityError MissingUsername
        => new(IdentityErrorCode.MissingUsername, "Missing username");

    public static IdentityError InvalidUsernameFormat
        => new(IdentityErrorCode.InvalidUsernameFormat, "Invalid username format");

    public static IdentityError UsernameDuplicated
        => new(IdentityErrorCode.UsernameDuplicated, "This username already exists");

    public static IdentityError MissingSecurityStamp
        => new(IdentityErrorCode.MissingSecurityStamp, "Missing security stamp");

    public static IdentityError InvalidSecurityStamp
        => new(IdentityErrorCode.InvalidSecurityStamp, "Invalid security stamp");

    public static IdentityError PasswordTooShort
        => new(IdentityErrorCode.PasswordTooShort, "Password is too short");

    public static IdentityError PasswordWithoutLowerCase
        => new(IdentityErrorCode.PasswordWithoutLowerCase, "Password must contain lower-case character");

    public static IdentityError PasswordWithoutUpperCase
        => new(IdentityErrorCode.PasswordWithoutUpperCase, "Password must contain upper-case character");

    public static IdentityError PasswordWithoutDigit
        => new(IdentityErrorCode.PasswordWithoutDigit, "Password must contain digit");

    public static IdentityError PasswordWithoutNonAlphanumeric
        => new(IdentityErrorCode.PasswordWithoutNonAlphanumeric, "Password must contain non-alphanumeric character");

    public static IdentityError InvalidPasswordResetCode
        => new(IdentityErrorCode.InvalidPasswordResetCode, "Invalid password reset code");

    public static IdentityError MissingPasswordResetCode
        => new(IdentityErrorCode.MissingPasswordResetCode, "Missing password reset code");

    public static IdentityError ExpiredPasswordResetCode
         => new(IdentityErrorCode.ExpiredPasswordResetCode, "Expired password reset code");

    public override bool Equals(object? obj)
    {
        if (obj is null || obj is not IdentityError error)
            return false;

        if (ReferenceEquals(this, error))
            return true;

        return ErrorCode.Equals(error.ErrorCode);
    }

    public override int GetHashCode() => ErrorCode.GetHashCode();
}
