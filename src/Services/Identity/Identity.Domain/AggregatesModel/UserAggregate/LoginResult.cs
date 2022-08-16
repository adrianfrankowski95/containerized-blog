using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class LoginResult : ValueObject<LoginResult>
{
    private static readonly LoginResult _success = new();
    public static ValidationResult<TItem> Success => _success;
    public bool IsSuccess { get; }
    public string? Error { get; }

    private LoginResult()
    {
        IsSuccess = true;
    }

    private LoginResult(NonEmptyString error)
    {
        if(error is null)
            throw new ArgumentNullException(nameof(error));

        IsSuccess = false;
        Error = error;
    }

    public static ValidationResult<TItem> Fail(IEnumerable<RequirementMessage<TItem>> errors) => new(errors);

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return IsSuccess;
        yield return Error;
    }
}