using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public class RecipePost : PostBase
{
    public Meal Meal { get; private set; }
    public RecipeDifficulty Difficulty { get; private set; }
    public RecipeTime Time { get; private set; }
    public Servings Servings { get; private set; }
    public FoodComposition FoodComposition { get; private set; }
    // This has to remain as a List, this is the only type that could be converted to text[] column in Postgres
    private readonly List<Taste> _tastes;
    public IReadOnlyList<Taste> Tastes => _tastes;
    // This has to remain as a List, this is the only type that could be converted to text[] column in Postgres
    private readonly List<PreparationMethod> _preparationMethods;
    public IReadOnlyList<PreparationMethod> PreparationMethods => _preparationMethods;
    public string SongUrl { get; private set; }

    public RecipePost(
        User author,
        IEnumerable<RecipePostTranslation> translations,
        Meal meal,
        RecipeDifficulty difficulty,
        RecipeTime time,
        Servings servings,
        FoodComposition composition,
        IEnumerable<Taste> tastes,
        IEnumerable<PreparationMethod> preparationMethods,
        string songUrl,
        string headerImgUrl)
        : base(author, PostCategory.Recipe, PostType.Recipe, translations, headerImgUrl)
    {
        Meal = meal;
        Difficulty = difficulty;
        Time = time;
        Servings = servings;
        FoodComposition = composition;
        SongUrl = songUrl;

        _tastes = new(tastes.Count());
        foreach (Taste taste in tastes)
            AddTaste(taste);

        _preparationMethods = new(preparationMethods.Count());
        foreach (PreparationMethod method in preparationMethods)
            AddPreparationMethod(method);
    }

    private void AddTaste(Taste taste)
    {
        if (taste is null)
            throw new BloggingDomainException($"Taste cannot be null or empty");

        if (!_tastes.Contains(taste))
            _tastes.Add(taste);
    }

    private void AddPreparationMethod(PreparationMethod method)
    {
        if (method is null)
            throw new BloggingDomainException($"{nameof(PreparationMethod)} cannot be null or empty");

        if (!_preparationMethods.Contains(method))
            _preparationMethods.Add(method);
    }

    public virtual bool UpdateBy(
        User editor,
        IEnumerable<RecipePostTranslation> newTranslations,
        Meal newMeal,
        RecipeDifficulty newDifficulty,
        RecipeTime newTime,
        Servings newServings,
        FoodComposition newComposition,
        IEnumerable<Taste> newTastes,
        IEnumerable<PreparationMethod> newPreparationMethods,
        string newSongUrl,
        string newHeaderImgUrl)
    {
        bool isChanged = false;

        if (!Meal.Equals(newMeal))
        {
            Meal = newMeal;
            isChanged = true;
        }

        if (!Difficulty.Equals(newDifficulty))
        {
            Difficulty = newDifficulty;
            isChanged = true;
        }

        if (!Time.Equals(newTime))
        {
            Time = newTime;
            isChanged = true;
        }

        if (!Servings.Equals(newServings))
        {
            Servings = newServings;
            isChanged = true;
        }

        if (!string.Equals(SongUrl, newSongUrl, StringComparison.OrdinalIgnoreCase))
        {
            SongUrl = newSongUrl;
            isChanged = true;
        }

        if (!FoodComposition.Equals(newComposition))
        {
            FoodComposition = newComposition;
            isChanged = true;
        }

        if (!_tastes.SequenceEqual(newTastes))
        {
            _tastes.Clear();

            foreach (var taste in newTastes)
                AddTaste(taste);

            isChanged = true;
        }

        if (!_preparationMethods.SequenceEqual(newPreparationMethods))
        {
            _preparationMethods.Clear();

            foreach (var method in newPreparationMethods)
                AddPreparationMethod(method);

            isChanged = true;
        }

        return base.UpdateBy(editor, newTranslations, newHeaderImgUrl) || isChanged;
    }

    public override void Validate()
    {
        if (Meal is null)
            throw new BloggingDomainException($"{nameof(Meal)} cannot be null");

        if (Time is null)
            throw new BloggingDomainException($"{nameof(Time)} cannot be null");

        if (Time.PreparationTime.ToMinutes() <= 0.Minutes())
            throw new BloggingDomainException($"{nameof(Time.PreparationTime)} cannot be nagative or equal to 0 minutes");

        if (Time.CookingTime.ToMinutes() < 0.Minutes())
            throw new BloggingDomainException($"{nameof(Time.CookingTime)} cannot be negative");

        if (Servings is null)
            throw new BloggingDomainException($"{nameof(Servings)} cannot be null");

        if (Servings.Count <= 0)
            throw new BloggingDomainException($"{nameof(Servings.Count)} cannot be negative or zero");

        if (FoodComposition is null)
            throw new BloggingDomainException($"{nameof(FoodComposition)} cannot be null");

        if (_tastes is null)
            throw new BloggingDomainException($"{nameof(Tastes)} cannot be null");

        if (_tastes.Count == 0)
            throw new BloggingDomainException($"{nameof(Tastes)} cannot be empty");

        if (_preparationMethods is null)
            throw new BloggingDomainException($"{nameof(PreparationMethods)} cannot be null");

        if (_preparationMethods.Count == 0)
            throw new BloggingDomainException($"{nameof(PreparationMethods)} cannot be empty");

        base.Validate();
    }
}
