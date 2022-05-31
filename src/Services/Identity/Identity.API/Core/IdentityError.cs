namespace Blog.Services.Identity.API.Core;

public abstract class IdentityError
{
    public IdentityErrorCode ErrorCode { get; }
    public string ErrorDescription { get; }

    protected IdentityError(IdentityErrorCode errorCode, string errorDescription)
    {
        ErrorCode = errorCode;
        ErrorDescription = errorDescription;
    }

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
