using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class PreparationMethod : Enumeration
{
    private PreparationMethod(int value, string name) : base(value, name) { }

    public static readonly PreparationMethod Undefined = new(-1, nameof(Undefined).ToLowerInvariant());
    public static readonly PreparationMethod None = new(0, nameof(None).ToLowerInvariant());
    public static readonly PreparationMethod Baking = new(1, nameof(Baking).ToLowerInvariant());
    public static readonly PreparationMethod Frying = new(2, nameof(Frying).ToLowerInvariant());
    public static readonly PreparationMethod Boiling = new(3, nameof(Boiling).ToLowerInvariant());
    public static readonly PreparationMethod Steaming = new(4, nameof(Steaming).ToLowerInvariant());

    public static IEnumerable<PreparationMethod> List() => new[] {
        Undefined,
        None,
        Baking,
        Frying,
        Boiling,
        Steaming };
    public static PreparationMethod FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var method = List()
            .SingleOrDefault(m => string.Equals(m.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (method is null)
            throw new BloggingDomainException($"Possible {nameof(PreparationMethod)} names: {string.Join(", ", List().Select(m => m.Name))}. Provided name: {name}");

        return method;
    }

    public static PreparationMethod FromValue(int value)
    {
        var method = List()
            .SingleOrDefault(l => l.Value == value);

        if (method is null)
            throw new BloggingDomainException($"Possible {nameof(PreparationMethod)} values: {string.Join(',', List().Select(m => m.Value))}. Provided value: {value}");

        return method;
    }
}
