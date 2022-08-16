using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class LoginAttempts : ValueObject<LoginAttempts>
{
    private readonly NonNegativeInt _count;
    private static readonly LoginAttempts _none = new();
    public const int MaxAllowed = 5;

    private LoginAttempts()
    {
        _count = 0;
    }

    private LoginAttempts(NonNegativeInt count)
    {
        if(count is null)
            throw new ArgumentNullException("Login attempts count must not be null.");

        _count = count;
    }
    public static LoginAttempts None => _none;
    public LoginAttempts Increment()
    {
        if (_count == MaxAllowed)
            throw new IdentityDomainException($"Maximum allowed login attempts are {MaxAllowed}.");

        return new(_count + 1);
    }

    public static bool operator >(LoginAttempts a, int b) => a._count > b;
    public static bool operator <(LoginAttempts a, int b) => a._count < b;
    public static bool operator ==(LoginAttempts a, int b) => a._count == b;
    public static bool operator !=(LoginAttempts a, int b) => a._count != b;

    public override bool Equals(LoginAttempts? second) => base.Equals(second);
    public override bool Equals(object? second) => base.Equals(second);
    public override int GetHashCode() => base.GetHashCode();

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _count;
    }
}