using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.Exceptions;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public class ConfirmEmailAddressCommandHandler : IRequestHandler<ConfirmEmailAddressCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ISysTime _sysTime;

    public ConfirmEmailAddressCommandHandler(
        IUserRepository userRepository,
        ISysTime sysTime)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
    }

    public async Task<Unit> Handle(ConfirmEmailAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByEmailAsync(request.EmailAddress).ConfigureAwait(false);
        if (user is null)
            throw new IdentityDomainException("Error confirming an email address.");

        user.ConfirmEmailAddress(request.EmailConfirmationCode, _sysTime.Now);
        await _userRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}