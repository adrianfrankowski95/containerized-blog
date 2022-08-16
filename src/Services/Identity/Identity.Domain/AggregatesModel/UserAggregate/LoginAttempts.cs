using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class LoginAttempts : ValueObject<LoginAttempts>
{
    private readonly NonNegativeInt _count;
    private static readonly LoginAttempts _zero = new();
    private static readonly LoginAttempts _maxAllowed = new();

    private LoginAttempts()
    {
        _count = 0;
    }

    private LoginAttempts(NonNegativeInt count)
    {
        _count = count;
    }
    public static LoginAttempts Zero => _zero;
    public static LoginAttempts MaxAllowed => _maxAllowed;
    public LoginAttempts Increment()
    {
        if (_count == MaxAllowed)
            throw new IdentityDomainException($"Maximum allowed failed login attempts are {MaxAllowed}.");

        return new(_count + 1);
    }

    public static bool operator >(NonNegativeInt a, LoginAttempts b) => a > b._count;
    public static bool operator <(NonNegativeInt a, LoginAttempts b) => a < b._count;
    public static bool operator ==(NonNegativeInt a, LoginAttempts b) => a == b._count;
    public static bool operator !=(NonNegativeInt a, LoginAttempts b) => a != b._count;

    public override bool Equals(LoginAttempts? second) => base.Equals(second);
    public override bool Equals(object? second) => base.Equals(second);
    public override int GetHashCode() => base.GetHashCode();

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _count;
    }
}