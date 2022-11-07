using Blog.Services.Identity.API.Infrastructure.Services;
using Blog.Services.Identity.Domain.Events;
using Blog.Services.Identity.Domain.Exceptions;
using MediatR;

namespace Blog.Services.Identity.API.Application.DomainEventHandlers.PasswordReset;

public class SendPasswordResetEmailDomainEventHandler : INotificationHandler<PasswordResetRequestedDomainEvent>
{
    private readonly IEmailingService _emailingService;
    private readonly ICallbackUrlGenerator _callbackUrlGenerator;
    private readonly ILogger<SendPasswordResetEmailDomainEventHandler> _logger;

    public SendPasswordResetEmailDomainEventHandler(
        IEmailingService emailingService,
        ICallbackUrlGenerator callbackUrlGenerator,
        ILogger<SendPasswordResetEmailDomainEventHandler> logger)
    {
        _emailingService = emailingService ?? throw new ArgumentNullException(nameof(emailingService));
        _callbackUrlGenerator = callbackUrlGenerator ?? throw new ArgumentNullException(nameof(callbackUrlGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(PasswordResetRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        var passwordResetCode = (notification.PasswordResetCode?.IsEmpty ?? true)
            ? throw new ArgumentException("Password reset code must not be empty.")
            : notification.PasswordResetCode;

        var urlExpiration = passwordResetCode.ValidUntil ?? throw new InvalidDataException("Invalid password reset code validity period.");

        var callbackUrl = _callbackUrlGenerator.GeneratePasswordResetCallbackUrl(passwordResetCode);

        var success = await _emailingService.SendPasswordResetEmailAsync(
            notification.Username,
            notification.EmailAddress,
            callbackUrl,
            urlExpiration)
            .ConfigureAwait(false);

        if (success)
        {
            _logger.LogInformation("----- Successfully sent a password reset email; callback URL: {CallbackUrl}, notification: {Notification}",
                callbackUrl, notification);
        }
        else
        {
            _logger.LogError("----- Error sending a password reset email; callback URL: {CallbackUrl}, notification: {Notification}",
                callbackUrl, notification);
            throw new EmailingServiceException("Error sending a password reset email.");
        }
    }
}
