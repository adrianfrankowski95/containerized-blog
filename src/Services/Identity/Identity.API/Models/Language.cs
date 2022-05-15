namespace Blog.Services.Identity.API.Models;

public class Language
{
    public int Value { get; }
    public NonEmptyString Name { get; }

    private Language(int value, NonEmptyString name)
    {
        Value = value;
        Name = name;
    }

    public static readonly Language Polish = new(0, nameof(Polish).ToLowerInvariant());
    public static readonly Language English = new(1, nameof(English).ToLowerInvariant());

    public static Language GetDefault() => Polish;
    public static IEnumerable<Language> List() => new[] {
        Polish,
        English };

    public static Language FromName(NonEmptyString name)
    {
        var language = List()
            .SingleOrDefault(c => string.Equals(c.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (language is null)
            throw new InvalidOperationException($"Possible {nameof(Language)} names: {string.Join(", ", List().Select(c => c.Name))}. Provided name: {name}");

        return language;
    }

    public static Language FromValue(int value)
    {
        if (value == null)
            throw new InvalidOperationException($"{nameof(Value)} cannot be null");

        var language = List()
            .SingleOrDefault(c => c.Value == value);

        if (language is null)
            throw new InvalidOperationException($"Possible {nameof(Language)} values: {string.Join(',', List().Select(c => c.Value))}. Provided value: {value}");

        return language;
    }
    public override string ToString() => Name;
}
