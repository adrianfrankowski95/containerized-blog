using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class LoginResult : ValueObject<LoginResult>
{
    public LoginErrorCode? ErrorCode { get; private set; }
    public bool IsSuccess => ErrorCode is null;
    private static readonly LoginResult _success = new();
    public static LoginResult Success => _success;

    private LoginResult()
    { }

    private LoginResult(LoginErrorCode code)
    {
        ErrorCode = code;
    }
    public static LoginResult Fail(LoginErrorCode code) => new(code);

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return ErrorCode;
    }
}

public enum LoginErrorCode
{
    AccountLockedOut = 0,
    AccountSuspended = 1,
    InvalidEmail = 2,
    UnconfirmedEmail = 3,
    InactivePassword = 4,
    InvalidPassword = 5,
}

