namespace Blog.Services.Identity.API.Core;

public class CredentialsError : IdentityError
{
    private CredentialsError(IdentityErrorCode errorCode, string errorDescription)
        : base(errorCode, errorDescription) { }

    public static CredentialsError InvalidCredentials
        => new(IdentityErrorCode.InvalidCredentials, "The Email and/or Password is invalid.");
}