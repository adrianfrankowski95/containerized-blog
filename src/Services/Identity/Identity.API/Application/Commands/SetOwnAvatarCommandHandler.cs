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
    private readonly IAvatarManager _avatarManager;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly ILogger<SetOwnAvatarCommandHandler> _logger;

    public SetOwnAvatarCommandHandler(
        IAvatarUploadService avatarUploadService,
        IAvatarManager avatarManager,
        IUserRepository userRepository,
        IIdentityService identityService,
        ILogger<SetOwnAvatarCommandHandler> logger)
    {
        _avatarUploadService = avatarUploadService ?? throw new ArgumentNullException(nameof(avatarUploadService));
        _avatarManager = avatarManager ?? throw new ArgumentNullException(nameof(avatarManager));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(SetOwnAvatarCommand request, CancellationToken cancellationToken)
    {
        if (!_identityService.IsAuthenticated)
            throw new IdentityDomainException("User is not authenticated.");

        bool userExists = await _userRepository.FindByIdAsync(_identityService.UserId!).ConfigureAwait(false) is not null;

        if (!userExists)
        {
            _logger.LogInformation("----- Deleting avatar of a user that no longer exists, user ID: {UserId}", _identityService.UserId);
            await _avatarManager.DeleteAvatarAsync(_identityService.UserId!.Value).ConfigureAwait(false);
            throw new IdentityDomainException("Could not find user with existing ID.");
        }

        await _avatarUploadService
            .UploadAvatarAsync(_identityService.UserId!.Value, request.ImageFile, cancellationToken)
            .ConfigureAwait(false);

        return Unit.Value;
    }
}