using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.Shared;

public class Language : Enumeration
{
    private Language(int value, string name) : base(value, name) { }

    public static readonly Language Polish = new(0, nameof(Polish).ToLowerInvariant());
    public static readonly Language English = new(1, nameof(English).ToLowerInvariant());

    public static Language GetDefault() => Polish;
    public static IEnumerable<Language> List() => new[] {
        Polish,
        English };

    public static Language FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var language = List()
            .SingleOrDefault(c => string.Equals(c.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (language is null)
            throw new BloggingDomainException($"Possible {nameof(Language)} names: {string.Join(", ", List().Select(c => c.Name))}. Provided name: {name}");

        return language;
    }

    public static Language FromValue(int value)
    {
        if (value == null)
            throw new BloggingDomainException($"{nameof(Value)} cannot be null");

        var language = List()
            .SingleOrDefault(c => c.Value == value);

        if (language is null)
            throw new BloggingDomainException($"Possible {nameof(Language)} values: {string.Join(',', List().Select(c => c.Value))}. Provided value: {value}");

        return language;
    }
}
