using Blog.Services.Blogging.API.Application.Models;
using Blog.Services.Blogging.API.Infrastructure.Services;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.LifestylePostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public class CreateAndPublishLifestylePostCommandHandler : IRequestHandler<CreateAndPublishLifestylePostCommand, Unit>
{
    private readonly IPostRepository _postRepository;
    private readonly IIdentityService _identityService;
    private readonly ITagRepository _tagRepository;

    public CreateAndPublishLifestylePostCommandHandler(
        IPostRepository postRepository,
        ITagRepository tagRepository,
        IIdentityService identityService)
    {
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }
    public async Task<Unit> Handle(CreateAndPublishLifestylePostCommand request, CancellationToken cancellationToken)
    {
        var user = _identityService.GetCurrentUser();

        var tagIds = request.Translations.SelectMany(x => x.TagIds).Select(x => new TagId(x)).ToList();

        IEnumerable<Tag> tags = tagIds is null
            ? Enumerable.Empty<Tag>()
            : await _tagRepository.FindTagsByIdsAsync(tagIds).ConfigureAwait(false);

        LifestylePost post;
        var translations = MapTranslations(request.Translations, tags);

        post = new LifestylePost(
            user,
            translations,
            request.HeaderImgUrl);

        post.PublishBy(user);
        _postRepository.AddPost(post);

        await _postRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
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