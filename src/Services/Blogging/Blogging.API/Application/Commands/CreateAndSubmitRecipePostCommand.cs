using Blog.Services.Blogging.API.Application.Commands.Models;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record CreateAndSubmitRecipePostCommand(
    string HeaderImgUrl,

    string Meal,
    string Difficulty,
    int PreparationMinutes,
    int CookingMinutes,
    int Servings,
    string FoodComposition,
    string SongUrl,
    IList<string> Tastes,
    IList<string> PreparationMethods,

    IList<RecipePostTranslationDTO> Translations) : IRequest<Unit>;