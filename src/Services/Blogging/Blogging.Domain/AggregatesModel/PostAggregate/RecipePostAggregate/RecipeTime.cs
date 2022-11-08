using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class RecipeTime : ValueObject<RecipeTime>
{
    public TimeSpan PreparationTime { get; }
    public TimeSpan CookingTime { get; }
    public TimeSpan TotalTime => PreparationTime + CookingTime;

    // EF Core
    private RecipeTime() { }
    public RecipeTime(TimeSpan preparationTime, TimeSpan cookingTime)
    {
        if (preparationTime is null)
            throw new BloggingDomainException($"{nameof(PreparationTime)} cannot be null");

        if (cookingTime is null)
            throw new BloggingDomainException($"{nameof(CookingTime)} cannot be null");

        if (preparationTime.ToMinutes() < 0.Minutes())
            throw new BloggingDomainException($"{nameof(PreparationTime)} cannot be negative");

        if (cookingTime.ToMinutes() < 0.Minutes())
            throw new BloggingDomainException($"{nameof(CookingTime)} cannot be negative");

        PreparationTime = preparationTime;
        CookingTime = cookingTime;
    }
    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return PreparationTime;
        yield return CookingTime;
    }

    public void Validate()
    {
        if (PreparationTime is null)
            throw new BloggingDomainException($"{nameof(PreparationTime)} cannot be null");

        if (CookingTime is null)
            throw new BloggingDomainException($"{nameof(CookingTime)} cannot be null");

        if (PreparationTime.ToMinutes() <= 0.Minutes())
            throw new BloggingDomainException($"{nameof(PreparationTime)} cannot be negative or zero");

        if (CookingTime.ToMinutes() < 0.Minutes())
            throw new BloggingDomainException($"{nameof(CookingTime)} cannot be negative");

    }
}
