namespace Blog.Services.Identity.API.Core;

public class IdentityResult
{
    private static readonly IdentityResult _success = new(true);
    public bool Succeeded { get; private set; }
    public IEnumerable<IdentityError> Errors { get; }

    private IdentityResult(bool success) { Succeeded = success; }

    private IdentityResult(IEnumerable<IdentityError> errors) : this(false)
    {
        Errors = errors;
    }

    public static IdentityResult Fail(IdentityError error) => new(new[] { error });
    public static IdentityResult Fail(IEnumerable<IdentityError> errors) => new(errors);
    public static IdentityResult Success => _success;

    public override bool Equals(object? obj)
    {
        if (obj is null || obj is not IdentityResult result)
            return false;

        if (ReferenceEquals(obj, this))
            return true;

        if (Succeeded && result.Succeeded)
            return true;

        if (Succeeded != result.Succeeded)
            return false;

        return Errors.SequenceEqual(result.Errors);
    }

    public override int GetHashCode() => Succeeded.GetHashCode();
}
