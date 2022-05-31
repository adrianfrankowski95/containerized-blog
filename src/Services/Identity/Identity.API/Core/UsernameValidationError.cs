namespace Blog.Services.Identity.API.Core;

public class UsernameValidationError : IdentityError
{
    private UsernameValidationError(IdentityErrorCode errorCode, string errorDescription)
        : base(errorCode, errorDescription) { }

    public static UsernameValidationError MissingUsername
        => new(IdentityErrorCode.MissingUsername, "The Username is requred.");

    public static UsernameValidationError InvalidUsernameFormat
        => new(IdentityErrorCode.InvalidUsernameFormat, "The Username has invalid format.");

    public static UsernameValidationError UsernameDuplicated
        => new(IdentityErrorCode.UsernameDuplicated, "The Username is already in use.");

    public static UsernameValidationError NewAndOldUsernamesAreEqual
        => new(IdentityErrorCode.NewAndOldUsernamesAreEqual, "The New username and Old username must be different.");
}