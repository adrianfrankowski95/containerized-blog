namespace Blog.Services.Identity.API.Core;

public class IdentityResult : IIdentityResult
{
    private static readonly IdentityResult _success = new(true);
    public bool Succeeded { get; private set; }
    public IEnumerable<IdentityError> Errors { get; }

    private IdentityResult(bool success) { Succeeded = success; }

    private IdentityResult(IdentityError error) : this(false)
    {
        Errors = new[] { error };
    }

    private IdentityResult(IEnumerable<IdentityError> errors) : this(false)
    {
        Errors = errors;
    }

    public static IdentityResult Fail(IdentityError error) => new(error);
    public static IdentityResult Fail(IEnumerable<IdentityError> errors) => new(errors);

    public static IdentityResult Success => _success;
}
