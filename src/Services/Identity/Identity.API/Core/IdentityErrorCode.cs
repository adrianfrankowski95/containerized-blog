namespace Blog.Services.Identity.API.Core;

public enum IdentityErrorCode
{
    InvalidCredentials = 0,
    InvalidIdentifier = 1,
    AccountLockedOut = 2,
    AccountSuspended = 3,
    ResettingPassword = 4,
    MissingEmail = 5,
    InvalidEmailFormat = 6,
    EmailDuplicated = 7,
    EmailUnconfirmed = 8,
    EmailAlreadyConfirmed = 9,
    InvalidEmailConfirmationCode = 10,
    MissingEmailConfirmationCode = 11,
    ExpiredEmailConfirmationCode = 12,
    MissingUsername = 13,
    InvalidUsernameFormat = 14,
    UsernameDuplicated = 15,
    MissingSecurityStamp = 16,
    InvalidSecurityStamp = 17,
    PasswordTooShort = 18,
    PasswordWithoutLowerCase = 19,
    PasswordWithoutUpperCase = 20,
    PasswordWithoutDigit = 21,
    PasswordWithoutNonAlphanumeric = 22,
    InvalidPasswordResetCode = 23,
    MissingPasswordResetCode = 24,
    ExpiredPasswordResetCode = 25
}