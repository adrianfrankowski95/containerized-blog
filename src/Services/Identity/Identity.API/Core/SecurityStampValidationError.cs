namespace Blog.Services.Identity.API.Core;

public class SecurityStampValidationError : IdentityError
{
    private SecurityStampValidationError(IdentityErrorCode errorCode, string errorDescription)
        : base(errorCode, errorDescription) { }

    public static SecurityStampValidationError MissingSecurityStamp
        => new(IdentityErrorCode.MissingSecurityStamp, "The Security stamp is missing.");

    public static SecurityStampValidationError InvalidSecurityStamp
        => new(IdentityErrorCode.InvalidSecurityStamp, "The Security stamp is not valid.");

}