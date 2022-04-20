using Blog.Services.Blogging.Domain.Exceptions;
using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class Ingredient : ValueObject<Ingredient>
{
    public string Name { get; }
    public string Amount { get; }
    public Ingredient(string amount, string name)
    {
        if (string.IsNullOrWhiteSpace(amount))
            throw new BloggingDomainException($"{nameof(Amount)} cannot be null");

        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null");

        Amount = amount;
        Name = name;
    }

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return Name;
        yield return Amount;
    }
}
