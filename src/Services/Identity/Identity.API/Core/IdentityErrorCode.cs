namespace Blog.Services.Identity.API.Core;

public enum IdentityErrorCode
{
    InvalidCredentials = 0,
    AccountLocked = 1,
    AccountSuspended = 2,
    UnconfirmedEmail = 3,
    ResettingPassword = 4,
    InvalidOrMissingSecurityStamp = 5,
    InvalidIdentifier = 6,
    InvalidPassword = 7,
    InvalidUsername = 8,
    InvalidEmail = 9
}