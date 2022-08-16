using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class Username : ValueObject<Username>
{
    private readonly NonEmptyString _value;

    private Username(NonEmptyString value)
    {
        _value = value;
    }

    public static Username Create(NonEmptyString input, IEnumerable<IRequirement<Username>> requirements)
    {
        var username = new Username(input);
        var result = new UsernameValidator(requirements).Validate(username);

        if (!result.IsSuccess)
            throw new IdentityDomainException(result.Error!);

        return username;
    }

    public bool Any(Func<char, bool> predicate) => _value.Any(predicate);
    public NonNegativeInt Length => _value.Length;

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
