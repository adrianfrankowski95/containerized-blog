using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class LoginAttempts : ValueObject<LoginAttempts>
{
    private readonly NonNegativeInt _count;
    public static readonly LoginAttempts MaxAllowed = new(5);

    private LoginAttempts()
    {
        _count = 0;
    }

    private LoginAttempts(NonNegativeInt count)
    {
        if (count > MaxAllowed)
            throw new IdentityDomainException($"Maximum allowed failed login attempts are {MaxAllowed}.");

        _count = count;
    }
    public static LoginAttempts None() => new();
    public LoginAttempts Increment() => new(_count + 1);

    public static bool operator >(NonNegativeInt a, LoginAttempts b) => a > b._count;
    public static bool operator <(NonNegativeInt a, LoginAttempts b) => a < b._count;

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return _count;
    }
}