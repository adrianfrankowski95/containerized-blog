using Blog.Services.Blogging.API.Application.Commands.Models;
using Blog.Services.Blogging.API.Application.Models;
using Blog.Services.Blogging.API.Infrastructure.Services;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.LifestylePostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public class UpdateAndSubmitLifestylePostCommandHandler : IRequestHandler<UpdateAndSubmitLifestylePostCommand, ICommandResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IIdentityService _identityService;
    private readonly ITagRepository _tagRepository;

    public UpdateAndSubmitLifestylePostCommandHandler(
        IPostRepository postRepository,
        ITagRepository tagRepository,
        IIdentityService identityService)
    {
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }
    public async Task<ICommandResult> Handle(UpdateAndSubmitLifestylePostCommand request, CancellationToken cancellationToken)
    {
        if (!_identityService.TryGetAuthenticatedUser(out User user))
            return CommandResult.IdentityError();

        var post = await _postRepository.FindPostAsync(new PostId(request.PostId)).ConfigureAwait(false);

        if (post is null)
            return CommandResult.NotFoundError(request.PostId);

        if (post is not LifestylePost lifestylePost)
            return CommandResult.IncorrectPostTypeError(post.Type, PostType.Lifestyle);

        var tagIds = request.Translations.SelectMany(x => x.TagIds).Select(x => new TagId(x));
        IEnumerable<Tag> tags = tagIds is null ?
            Enumerable.Empty<Tag>() :
            await _tagRepository.FindTagsByIdsAsync(tagIds).ConfigureAwait(false);

        bool isChanged;
        try
        {
            var translations = MapTranslations(request.Translations, tags);

            isChanged = lifestylePost.UpdateBy(
                user,
                translations,
                request.HeaderImgUrl);

            lifestylePost.SubmitBy(user);
        }
        catch (Exception ex)
        {
            return CommandResult.DomainError(ex.Message);
        }

        if (isChanged)
            if (!await _postRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false))
                return CommandResult.SavingError();

        return CommandResult.Success();
    }

    private static IEnumerable<LifestylePostTranslation> MapTranslations(
        IEnumerable<LifestylePostTranslationDTO> requestTranslations,
        IEnumerable<Tag> tags)
    {
        List<LifestylePostTranslation> translations = new();

        foreach (var requestTranslation in requestTranslations ?? Enumerable.Empty<LifestylePostTranslationDTO>())
        {
            translations.Add(
                new LifestylePostTranslation(
                    Language.FromName(requestTranslation.Language),
                    requestTranslation.Title,
                    requestTranslation.Content,
                    requestTranslation.Description,
                    tags.Where(x => requestTranslation.TagIds.Contains(x.Id.Value))));
        }

        return translations;
    }
}