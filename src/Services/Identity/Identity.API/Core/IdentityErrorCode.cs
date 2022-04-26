namespace Blog.Services.Identity.API.Core;

public enum IdentityErrorCode
{
    InvalidCredentials = 0,
    InvalidIdentifier = 1,
    AccountLocked = 2,
    AccountSuspended = 3,
    ResettingPassword = 4,
    MissingEmail = 5,
    InvalidEmailFormat = 6,
    EmailDuplicated = 7,
    EmailUnconfirmed = 8,
    MissingUsername = 9,
    InvalidUsernameFormat = 10,
    UsernameDuplicated = 11,
    MissingPassword = 12,
    InvalidPassword = 13,
    InvalidPasswordFormat = 14,
    PasswordOkNeedsRehash = 15,
    MissingSecurityStamp = 16,
    InvalidSecurityStamp = 17
}