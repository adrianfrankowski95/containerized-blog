using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.Exceptions;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ISysTime _sysTime;

    public RequestPasswordResetCommandHandler(
        IUserRepository userRepository,
        ISysTime sysTime)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
    }

    public async Task<Unit> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByEmailAsync(request.EmailAddress).ConfigureAwait(false);
        if (user is null)
            throw new IdentityDomainException("Error resetting password.");

        user.RequestPasswordReset(_sysTime.Now);
        await _userRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}