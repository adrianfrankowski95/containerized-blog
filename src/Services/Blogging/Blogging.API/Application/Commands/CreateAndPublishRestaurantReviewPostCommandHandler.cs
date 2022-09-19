using Blog.Services.Blogging.API.Application.Models;
using Blog.Services.Blogging.API.Infrastructure.Services;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Blogging.API.Application.Commands;

public class CreateAndPublishRestaurantReviewPostCommandHandler : IRequestHandler<CreateAndPublishRestaurantReviewPostCommand, Unit>
{
    private readonly IPostRepository _postRepository;
    private readonly IIdentityService _identityService;
    private readonly ITagRepository _tagRepository;

    public CreateAndPublishRestaurantReviewPostCommandHandler(
        IPostRepository postRepository,
        ITagRepository tagRepository,
        IIdentityService identityService)
    {
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }
    public async Task<Unit> Handle(CreateAndPublishRestaurantReviewPostCommand request, CancellationToken cancellationToken)
    {
        var user = _identityService.GetCurrentUser();

        var tagIds = request.Translations.SelectMany(x => x.TagIds).Select(x => new TagId(x)).ToList();
        IEnumerable<Tag> tags = tagIds is null ?
            Enumerable.Empty<Tag>() :
            await _tagRepository.FindTagsByIdsAsync(tagIds).ConfigureAwait(false);

        var translations = MapTranslations(request.Translations, tags);

        var post = new RestaurantReviewPost(
            user,
            translations,
            new Restaurant(request.RestaurantName, request.RestaurantWebsiteUrl),
            new Rating(request.Rating),
            request.HeaderImgUrl);

        post.PublishBy(user);
        _postRepository.AddPost(post);

        await _postRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }

    private static IEnumerable<RestaurantReviewPostTranslation> MapTranslations(
        IEnumerable<RestaurantReviewPostTranslationDTO> requestTranslations,
        IEnumerable<Tag> tags)
    {
        List<RestaurantReviewPostTranslation> translations = new();

        foreach (var requestTranslation in requestTranslations ?? Enumerable.Empty<RestaurantReviewPostTranslationDTO>())
        {
            translations.Add(
                new RestaurantReviewPostTranslation(
                    Language.FromName(requestTranslation.Language),
                    requestTranslation.Title,
                    requestTranslation.Content,
                    requestTranslation.Description,
                    tags.Where(x => requestTranslation.TagIds.Contains(x.Id.Value)),
                    new Address(
                        requestTranslation.RestaurantAddressCountry,
                        requestTranslation.RestaurantAddressZipCode,
                        requestTranslation.RestaurantAddressCity,
                        requestTranslation.RestaurantAddressStreet),
                    requestTranslation.RestaurantCuisines));
        }

        return translations;
    }
}