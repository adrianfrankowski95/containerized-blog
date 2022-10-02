using Blog.Services.Identity.API.Infrastructure.Services;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Infrastructure.Avatar;
using Identity.API.Infrastructure.Services;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public class SetOwnAvatarCommandHandler : IRequestHandler<SetOwnAvatarCommand>
{
    private readonly IAvatarUploadService _avatarUploadService;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;

    public SetOwnAvatarCommandHandler(
        IAvatarUploadService avatarUploadService,
        IUserRepository userRepository,
        IIdentityService identityService)
    {
        _avatarUploadService = avatarUploadService ?? throw new ArgumentNullException(nameof(avatarUploadService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }

    public async Task<Unit> Handle(SetOwnAvatarCommand request, CancellationToken cancellationToken)
    {
        if (!_identityService.IsAuthenticated)
            throw new IdentityDomainException("User is not authenticated.");

        bool userExists = await _userRepository.FindByIdAsync(_identityService.UserId!).ConfigureAwait(false) is not null;

        if (!userExists)
            throw new IdentityDomainException("Could not find user with existing ID.");

        await _avatarUploadService
            .UploadAvatarAsync(_identityService.UserId!.Value, request.ImageFile, cancellationToken)
            .ConfigureAwait(false);

        return Unit.Value;
    }
}