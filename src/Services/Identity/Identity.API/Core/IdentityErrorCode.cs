namespace Blog.Services.Identity.API.Core;

public enum IdentityErrorCode
{
    InvalidCredentials = 0,
    AccountLocked = 1,
    AccountSuspended = 2,
    UnconfirmedEmail = 3,
    ResettingPassword = 4,
    InvalidIdentifier = 5,
    InvalidEmailFormat = 6,
    EmailDuplicated = 7,
    InvalidUsernameFormat = 8,
    UsernameDuplicated = 9,
    InvalidPasswordFormat = 10,
    PasswordOkNeedsRehash = 11
}