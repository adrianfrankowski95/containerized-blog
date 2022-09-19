using Blog.Services.Identity.API.Infrastructure.Services;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.Exceptions;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand>
{
    private readonly IIdentityService _identityService;
    private readonly LoginService _loginService;
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;
    private readonly ISysTime _sysTime;

    public RegisterCommandHandler(
        LoginService loginService,
        IIdentityService identityService,
        IUserRepository userRepository,
        PasswordHasher passwordHasher,
        ISysTime sysTime)
    {
        _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
    }

    public async Task<Unit> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (_identityService.IsAuthenticated)
            throw new IdentityDomainException("User is already authenticated.");

        bool emailInUse = await _userRepository.FindByEmailAsync(request.EmailAddress).ConfigureAwait(false) is not null;
        if (emailInUse)
            throw new IdentityDomainException("Provided email address is already in use.");

        bool usernameInUse = await _userRepository.FindByUsernameAsync(request.Username).ConfigureAwait(false) is not null;
        if (usernameInUse)
            throw new IdentityDomainException("Provided username is already in use.");

        var newUser = User.Register(
            new Username(request.Username),
            new FullName(request.FirstName, request.LastName),
            Gender.FromName(request.Gender),
            request.EmailAddress,
            _passwordHasher.HashPassword(request.Password),
            request.ReceiveAdditionalEmails,
            _sysTime.Now);

        _userRepository.AddUser(newUser);

        await _userRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }

    private string GetErrorMessage(LoginResult result, User user) => result switch
    {
        LoginResult { ErrorCode: LoginErrorCode.InvalidEmail or LoginErrorCode.InvalidPassword }
            => "Invalid email address and/or password.",
        LoginResult { ErrorCode: LoginErrorCode.AccountLockedOut }
            => "Account has temporarily been locked out. Please try again later.",
        LoginResult { ErrorCode: LoginErrorCode.AccountSuspended }
            => $"Account has been suspended until {user.SuspendedUntil}.",
        LoginResult { ErrorCode: LoginErrorCode.UnconfirmedEmail }
            => "Email address has not yet been confirmed.",
        LoginResult { ErrorCode: LoginErrorCode.InactivePassword }
            => "Password has not yet been reset.",
        _ => throw new NotSupportedException($"Unhandled {nameof(LoginResult)} error: {result.ErrorCode}.")
    };
}