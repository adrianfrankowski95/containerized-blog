using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class Servings : ValueObject<Servings>
{
    public int Count { get; }

    public Servings(int count)
    {
        if (count < 0)
            throw new BloggingDomainException($"{nameof(Count)} cannot be negative");

        Count = count;
    }

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return Count;
    }
}
