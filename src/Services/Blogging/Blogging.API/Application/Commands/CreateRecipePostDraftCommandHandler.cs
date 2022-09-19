using Blog.Services.Blogging.API.Application.Models;
using Blog.Services.Blogging.API.Infrastructure.Services;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using MediatR;
using TimeSpan = Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate.TimeSpan;

namespace Blog.Services.Blogging.API.Application.Commands;

public class CreateRecipePostDraftCommandHandler : IRequestHandler<CreateRecipePostDraftCommand, Unit>
{
    private readonly IPostRepository _postRepository;
    private readonly IIdentityService _identityService;
    private readonly ITagRepository _tagRepository;

    public CreateRecipePostDraftCommandHandler(
        IPostRepository postRepository,
        ITagRepository tagRepository,
        IIdentityService identityService)
    {
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }
    public async Task<Unit> Handle(CreateRecipePostDraftCommand request, CancellationToken cancellationToken)
    {
        var user = _identityService.GetCurrentUser();

        var tagIds = request.Translations.SelectMany(x => x.TagIds).Select(x => new TagId(x)).ToList();
        IEnumerable<Tag> tags = tagIds is null ?
            Enumerable.Empty<Tag>() :
            await _tagRepository.FindTagsByIdsAsync(tagIds).ConfigureAwait(false);

        var translations = MapTranslations(request.Translations, tags);

        var post = new RecipePost(
            user,
            translations,
            Meal.FromName(request.Meal),
            RecipeDifficulty.FromName(request.Difficulty),
            new RecipeTime(
                TimeSpan.FromMinutes(request.PreparationMinutes.Minutes()),
                TimeSpan.FromMinutes(request.CookingMinutes.Minutes())),
            new Servings(request.Servings),
            FoodComposition.FromName(request.FoodComposition),
            request.Tastes.Select(x => Taste.FromName(x)),
            request.PreparationMethods.Select(x => PreparationMethod.FromName(x)),
            request.SongUrl,
            request.HeaderImgUrl);

        _postRepository.AddPost(post);

        await _postRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }

    private static IEnumerable<RecipePostTranslation> MapTranslations(
        IEnumerable<RecipePostTranslationDTO> requestTranslations,
        IEnumerable<Tag> tags)
    {
        List<RecipePostTranslation> translations = new();

        foreach (var requestTranslation in requestTranslations ?? Enumerable.Empty<RecipePostTranslationDTO>())
        {
            translations.Add(
                new RecipePostTranslation(
                    Language.FromName(requestTranslation.Language),
                    requestTranslation.Title,
                    requestTranslation.Content,
                    requestTranslation.Description,
                    tags.Where(x => requestTranslation.TagIds.Contains(x.Id.Value)),
                    requestTranslation.DishName,
                    requestTranslation.Cuisine,
                    requestTranslation.Ingredients));
        }

        return translations;
    }
}