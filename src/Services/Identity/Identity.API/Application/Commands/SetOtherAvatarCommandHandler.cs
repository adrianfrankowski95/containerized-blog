using Blog.Services.Identity.API.Infrastructure.Services;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Infrastructure.Avatar;
using Identity.API.Infrastructure.Services;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public class SetOtherAvatarCommandHandler : IRequestHandler<SetOtherAvatarCommand>
{
    private readonly IAvatarUploadService _avatarUploadService;
    private readonly IAvatarManager _avatarManager;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly ILogger<SetOwnAvatarCommandHandler> _logger;

    public SetOtherAvatarCommandHandler(
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

    public async Task<Unit> Handle(SetOtherAvatarCommand request, CancellationToken cancellationToken)
    {
        if (!_identityService.IsAuthenticated)
            throw new IdentityDomainException("User is not authenticated.");

        if (!(_identityService.IsInRole(UserRole.Administrator) || _identityService.IsInRole(UserRole.Moderator)))
        {
            _logger.LogInformation("----- Refused to set avatar of another user due to unsufficient role: {Role}", _identityService.UserRole);
            throw new IdentityDomainException("User is not authorized to set another user's avatar.");
        }

        var user = await _userRepository.FindByEmailAsync(request.EmailAddress).ConfigureAwait(false);

        if (user is null)
            throw new IdentityDomainException($"Could not find user with provided email address: {request.EmailAddress}.");

        await _avatarUploadService
            .UploadAvatarAsync(user.Id.Value, request.ImageFile, cancellationToken)
            .ConfigureAwait(false);

        return Unit.Value;
    }
}