using Blog.Services.Blogging.API.Infrastructure.Services;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public class PublishPostCommandHandler : IRequestHandler<PublishPostCommand, Unit>
{
    private readonly IPostRepository _postRepository;
    private readonly IIdentityService _identityService;

    public PublishPostCommandHandler(IPostRepository postRepository, IIdentityService identityService)
    {
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }
    public async Task<Unit> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        var user = _identityService.GetCurrentUser();

        var post = await _postRepository.FindPostAsync(new PostId(request.PostId)).ConfigureAwait(false);

        if (post is null)
            throw new KeyNotFoundException("This post does not exist anymore");

        post.PublishBy(user);
        await _postRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}