using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public sealed class RequirementMessage<TItem> : ValueObject<RequirementMessage<TItem>>
{
    private readonly NonEmptyString _value;

    public RequirementMessage(NonEmptyString value)
    {
        if(value is null)
            throw new ArgumentNullException("Requirement message must not be null.");
            
        _value = value;
    }

    public static implicit operator RequirementMessage<TItem>(string value) => new(value);
    public static implicit operator string(RequirementMessage<TItem> value) => value._value;

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}