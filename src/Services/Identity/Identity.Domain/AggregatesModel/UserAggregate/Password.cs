using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class Password : ValueObject<Password>
{
    private readonly NonEmptyString _value;

    private Password(NonEmptyString value)
    {
        _value = value;
    }

    public static Password Create(NonEmptyString input, IEnumerable<IRequirement<Password>> requirements)
    {
        if(input is null)
            throw new ArgumentNullException("Password must not be null.");
            
        var password = new Password(input);
        var result = new PasswordValidator(requirements).Validate(password);

        if (!result.IsSuccess)
            throw new IdentityDomainException(result.Error!);

        return password;
    }

    public bool Any(Func<char, bool> predicate) => _value.Any(predicate);
    public NonNegativeInt Length => _value.Length;

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
