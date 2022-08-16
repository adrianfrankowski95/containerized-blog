using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class LoginResult : ValueObject<LoginResult>
{
    private static readonly LoginResult _success = new();
    public static LoginResult Success => _success;
    public LoginErrorCode? ErrorCode { get; private set; }
    public string? ErrorMessage { get; }
    public bool IsSuccess => ErrorCode is null;

    private LoginResult()
    { }

    private LoginResult(LoginErrorCode code)
    {
        ErrorCode = code;
        ErrorMessage = GetDefaultErrorMessage(code);
    }

    public static LoginResult Fail(LoginErrorCode code) => new(code);

    private static string GetDefaultErrorMessage(LoginErrorCode code) => code switch
    {
        LoginErrorCode.InvalidCredentials => "Invalid email address and/or password.",
        LoginErrorCode.AccountLockedOut => "Account has temporarily been locked out.",
        LoginErrorCode.AccountSuspended => "Account has been suspended.",
        LoginErrorCode.UnconfirmedEmail => "This email address has not yet been confirmed.",
        _ => throw new ArgumentException("Invalid login error code.")
    };

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return ErrorCode;
    }
}

public enum LoginErrorCode { AccountLockedOut = 0, AccountSuspended = 1, UnconfirmedEmail = 2, InvalidCredentials = 3 }

