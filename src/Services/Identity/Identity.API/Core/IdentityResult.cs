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
}
