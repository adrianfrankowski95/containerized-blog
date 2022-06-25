using Blog.Services.Blogging.API.Application.Commands.Models;
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

public class CreateAndPublishProductReviewPostCommandHandler : IRequestHandler<CreateAndPublishProductReviewPostCommand, ICommandResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IIdentityService _identityService;
    private readonly ITagRepository _tagRepository;

    public CreateAndPublishProductReviewPostCommandHandler(
        IPostRepository postRepository,
        ITagRepository tagRepository,
        IIdentityService identityService)
    {
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }
    public async Task<ICommandResult> Handle(CreateAndPublishProductReviewPostCommand request, CancellationToken cancellationToken)
    {
        if (!_identityService.TryGetAuthenticatedUser(out User user))
            return CommandResult.IdentityError();

        var tagIds = request.Translations.SelectMany(x => x.TagIds).Select(x => new TagId(x)).ToList();
        IEnumerable<Tag> tags = tagIds is null ?
            Enumerable.Empty<Tag>() :
            await _tagRepository.FindTagsByIdsAsync(tagIds).ConfigureAwait(false);

        ProductReviewPost post;
        try
        {
            var translations = MapTranslations(request.Translations, tags);

            post = new ProductReviewPost(
                user,
                translations,
                new Product(request.ProductName, request.ProductWebsiteUrl),
                new Rating(request.Rating),
                request.HeaderImgUrl);

            post.PublishBy(user);
            _postRepository.AddPost(post);

            await _postRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is BloggingDomainException or ArgumentNullException)
        {
            return CommandResult.DomainError(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CommandResult.ConcurrencyError();
        }
        catch (DbUpdateException)
        {
            return CommandResult.SavingError();
        }

        return CommandResult.Success();
    }

    private static IEnumerable<ProductReviewPostTranslation> MapTranslations(
        IEnumerable<ProductReviewPostTranslationDTO> requestTranslations,
        IEnumerable<Tag> tags)
    {
        List<ProductReviewPostTranslation> translations = new();

        foreach (var requestTranslation in requestTranslations ?? Enumerable.Empty<ProductReviewPostTranslationDTO>())
        {
            translations.Add(
                new ProductReviewPostTranslation(
                    Language.FromName(requestTranslation.Language),
                    requestTranslation.Title,
                    requestTranslation.Content,
                    requestTranslation.Description,
                    tags.Where(x => requestTranslation.TagIds.Contains(x.Id.Value))));
        }

        return translations;
    }
}