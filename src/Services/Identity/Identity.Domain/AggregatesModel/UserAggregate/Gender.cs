using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class Gender : Enumeration
{

    private Gender(int value, NonEmptyString name) : base(value, name)
    {
    }

    public static readonly Gender Male = new(0, nameof(Male).ToLowerInvariant());
    public static readonly Gender Female = new(1, nameof(Female).ToLowerInvariant());

    public static IEnumerable<Gender> List() => new[] { Male, Female };

    public static Gender FromName(NonEmptyString name)
    {
        var gender = List()
            .SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

        if (gender is null)
            throw new InvalidOperationException($"Possible {nameof(Gender)} names: {string.Join(", ", List().Select(r => r.Name))}. Provided name: {name}");

        return gender;
    }

    public static Gender FromValue(int value)
    {
        var gender = List()
            .SingleOrDefault(r => r.Value == value);

        if (gender is null)
            throw new InvalidOperationException($"Possible {nameof(Gender)} values: {string.Join(',', List().Select(r => r.Value))}. Provided value: {value}");

        return gender;
    }
}
