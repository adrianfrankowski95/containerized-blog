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
        => new(IdentityErrorCode.InvalidCredentials, "The Email and/or Password is invalid.");

    public static IdentityError InvalidIdentifier
        => new(IdentityErrorCode.InvalidCredentials, "The identifier is not valid.");

    public static IdentityError AccountLockedOut
        => new(IdentityErrorCode.AccountLockedOut, "This account has been temporarily locked out.");

    public static IdentityError AccountSuspended
        => new(IdentityErrorCode.AccountSuspended, "This account has been suspended.");

    public static IdentityError ResettingPassword
        => new(IdentityErrorCode.ResettingPassword, "This account has requested a password reset.");

    public static IdentityError MissingEmail
        => new(IdentityErrorCode.MissingEmail, "The Email is required.");

    public static IdentityError InvalidEmailFormat
        => new(IdentityErrorCode.InvalidEmailFormat, "The Email has invalid format.");

    public static IdentityError EmailDuplicated
        => new(IdentityErrorCode.EmailDuplicated, "The Email is already in use.");

    public static IdentityError EmailUnconfirmed
        => new(IdentityErrorCode.EmailUnconfirmed, "The Email is unconfirmed.");

    public static IdentityError EmailAlreadyConfirmed
        => new(IdentityErrorCode.EmailAlreadyConfirmed, "The Email is already confirmed.");

    public static IdentityError InvalidEmailConfirmationCode
        => new(IdentityErrorCode.InvalidEmailConfirmationCode, "The Email Confirmation code is invalid.");

    public static IdentityError MissingEmailConfirmationCode
        => new(IdentityErrorCode.MissingEmailConfirmationCode, "The Email Confirmation code is required.");

    public static IdentityError ExpiredEmailConfirmationCode
         => new(IdentityErrorCode.ExpiredEmailConfirmationCode, "The Email Confirmation code has expired.");

    public static IdentityError MissingUsername
        => new(IdentityErrorCode.MissingUsername, "The Username is requred.");

    public static IdentityError InvalidUsernameFormat
        => new(IdentityErrorCode.InvalidUsernameFormat, "The Username has invalid format.");

    public static IdentityError UsernameDuplicated
        => new(IdentityErrorCode.UsernameDuplicated, "The Username is already in use.");

    public static IdentityError MissingSecurityStamp
        => new(IdentityErrorCode.MissingSecurityStamp, "The Security Stamp is missing.");

    public static IdentityError InvalidSecurityStamp
        => new(IdentityErrorCode.InvalidSecurityStamp, "The Security Stamp is not valid.");

    public static IdentityError PasswordTooShort
        => new(IdentityErrorCode.PasswordTooShort, "The Password is too short.");

    public static IdentityError PasswordWithoutLowerCase
        => new(IdentityErrorCode.PasswordWithoutLowerCase, "The Password must contain lower-case character.");

    public static IdentityError PasswordWithoutUpperCase
        => new(IdentityErrorCode.PasswordWithoutUpperCase, "The Password must contain upper-case character.");

    public static IdentityError PasswordWithoutDigit
        => new(IdentityErrorCode.PasswordWithoutDigit, "The Password must contain digit.");

    public static IdentityError PasswordWithoutNonAlphanumeric
        => new(IdentityErrorCode.PasswordWithoutNonAlphanumeric, "The Password must contain a non-alphanumeric character.");

    public static IdentityError InvalidPasswordResetCode
        => new(IdentityErrorCode.InvalidPasswordResetCode, "Invalid password reset code.");

    public static IdentityError MissingPasswordResetCode
        => new(IdentityErrorCode.MissingPasswordResetCode, "Missing password reset code.");

    public static IdentityError ExpiredPasswordResetCode
         => new(IdentityErrorCode.ExpiredPasswordResetCode, "Expired password reset code.");

    public static IdentityError PasswordResetNotRequested
        => new(IdentityErrorCode.PasswordResetNotRequested, "Password reset has not been requested.");

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
