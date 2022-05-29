namespace Blog.Services.Identity.API.Core;

public enum IdentityErrorCode
{
    InvalidCredentials = 0,
    AccountLockedOut = 1,
    AccountSuspended = 2,
    ResettingPassword = 3,
    MissingEmail = 4,
    InvalidEmailFormat = 5,
    EmailDuplicated = 6,
    EmailUnconfirmed = 7,
    EmailAlreadyConfirmed = 8,
    InvalidEmailConfirmationCode = 9,
    ExpiredEmailConfirmationCode = 10,
    MissingUsername = 11,
    InvalidUsernameFormat = 12,
    UsernameDuplicated = 13,
    MissingSecurityStamp = 14,
    InvalidSecurityStamp = 15,
    PasswordTooShort = 16,
    PasswordWithoutLowerCase = 17,
    PasswordWithoutUpperCase = 18,
    PasswordWithoutDigit = 19,
    PasswordWithoutNonAlphanumeric = 20,
    InvalidPasswordResetCode = 21,
    ExpiredPasswordResetCode = 22,
    PasswordResetNotRequested = 23,
}