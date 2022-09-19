namespace Blog.Services.Blogging.API.Application.Models;

public record RecipePostTranslationDTO(
    string Language,
    string Title,
    string Content,
    string Description,
    IList<Guid> TagIds,

    string DishName,
    string Cuisine,
    IList<string> Ingredients);