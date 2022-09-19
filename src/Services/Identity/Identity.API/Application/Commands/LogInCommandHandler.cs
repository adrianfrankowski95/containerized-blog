using Blog.Services.Identity.API.Infrastructure.Services;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.Exceptions;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public class LogInCommandHandler : IRequestHandler<LogInCommand>
{
    private readonly IIdentityService _identityService;
    private readonly LoginService _loginService;
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;
    private readonly ISysTime _sysTime;

    public LogInCommandHandler(
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

    public async Task<Unit> Handle(LogInCommand request, CancellationToken cancellationToken)
    {
        if (_identityService.IsAuthenticated)
            throw new IdentityDomainException("User is already authenticated.");

        var user = await _userRepository.FindByEmailAsync(request.EmailAddress).ConfigureAwait(false);

        if (user is null)
            throw new IdentityDomainException("Invalid email adress and/or password.");

        var result = user.LogIn(_loginService, request.EmailAddress, _passwordHasher.HashPassword(request.Password), _sysTime.Now);

        await _userRepository.UnitOfWork.CommitChangesAsync(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
            throw new IdentityDomainException(GetErrorMessage(result, user));

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