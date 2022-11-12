using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public class RecipePostTranslation : PostTranslationBase
{
    public string DishName { get; private set; }
    public string Cuisine { get; private set; }
    // This has to remain as a List, this is the only type that could be converted to text[] column in Postgres
    private readonly List<string> _ingredients;
    public IReadOnlyList<string> Ingredients => _ingredients;

    public RecipePostTranslation(
        Language language,
        string title,
        string content,
        string description,
        IEnumerable<Tag> tags,
        string dishName,
        string cuisine,
        IEnumerable<string> ingredients)
        : base(language, title, content, description, tags)
    {
        DishName = dishName;
        Cuisine = cuisine;

        _ingredients = new(ingredients.Count());
        foreach (string ingredient in ingredients)
            AddIngredient(ingredient);
    }

    private void AddIngredient(string ingredient)
    {
        string trimmedIngredient = ingredient.Trim();

        if (string.IsNullOrWhiteSpace(trimmedIngredient))
            throw new BloggingDomainException($"Ingredient cannot be null or empty");

        if (!_ingredients.Contains(trimmedIngredient))
            _ingredients.Add(trimmedIngredient);
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return DishName;
        yield return Cuisine;
        yield return Ingredients;
        yield return base.GetEqualityCheckAttributes();
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(DishName))
            throw new BloggingDomainException($"{nameof(DishName)} cannot be null or empty");

        if (string.IsNullOrWhiteSpace(Cuisine))
            throw new BloggingDomainException($"{nameof(Cuisine)} cannot be null or empty");

        if (Ingredients is null)
            throw new BloggingDomainException($"{nameof(Ingredients)} cannot be null");

        if (Ingredients.Count == 0)
            throw new BloggingDomainException($"{nameof(Ingredients)} cannot be empty");

        base.Validate();
    }
}
