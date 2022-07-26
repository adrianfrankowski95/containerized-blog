using Blog.Services.Blogging.API.Application.Exceptions;
using Blog.Services.Blogging.API.Infrastructure.Services;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.Exceptions;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public class ApprovePostCommandHandler : IRequestHandler<ApprovePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IIdentityService _identityService;

    public ApprovePostCommandHandler(IPostRepository postRepository, IIdentityService identityService)
    {
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }
    public async Task<Unit> Handle(ApprovePostCommand request, CancellationToken cancellationToken)
    {
        var user = _identityService.GetCurrentUser();

        var post = await _postRepository.FindPostAsync(new PostId(request.PostId)).ConfigureAwait(false);

        if (post is null)
            throw new KeyNotFoundException($"Could not find a post with the following ID: {request.PostId}");

        post.ApproveBy(user);
        await _postRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}