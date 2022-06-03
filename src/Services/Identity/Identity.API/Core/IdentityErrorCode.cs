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
    MissingPassword = 14,
    PasswordTooShort = 15,
    PasswordWithoutLowerCase = 16,
    PasswordWithoutUpperCase = 17,
    PasswordWithoutDigit = 18,
    PasswordWithoutNonAlphanumeric = 19,
    InvalidPasswordResetCode = 20,
    ExpiredPasswordResetCode = 21,
    NewAndOldPasswordsAreEqual = 22,
    NewAndOldEmailsAreEqual = 23,
    NewAndOldUsernamesAreEqual = 24
}