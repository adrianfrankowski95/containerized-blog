using Blog.Services.Emailing.API.Factories;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using Grpc.Core;
using NodaTime.Serialization.Protobuf;

namespace Blog.Services.Emailing.API.Grpc;

public class EmailingService : GrpcEmailingService.GrpcEmailingServiceBase
{
    private readonly ILogger<EmailingService> _logger;
    private readonly IEmailFactory<IFluentEmail> _emailFactory;
    private readonly ISender _sender;

    private readonly static SendEmailResponse _success = new() { Success = true };
    private readonly static SendEmailResponse _fail = new() { Success = false };

    public EmailingService(
        IEmailFactory<IFluentEmail> emailFactory,
        ISender sender,
        ILogger<EmailingService> logger)
    {
        _emailFactory = emailFactory ?? throw new ArgumentNullException(nameof(emailFactory));
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<SendEmailResponse> SendEmailConfirmationEmail(SendEmailConfirmationEmailRequest request, ServerCallContext context)
    {
        _logger.LogInformation("----- Sending confirmation email to {Name} <{EmailAddress}>", request.Recipient.Name, request.Recipient.EmailAddress);
        try
        {
            var email = _emailFactory.CreateEmailConfirmationEmail(
                new Models.Recipient(request.Recipient.EmailAddress, request.Recipient.Name),
                request.CallbackUrl,
                request.UrlExpirationAt.ToInstant());

            var result = await _sender.SendAsync(email).ConfigureAwait(false);

            if (result.Successful)
            {
                _logger.LogInformation("----- Confirmation email to {Name} <{EmailAddress}> sent sucessfully", request.Recipient.Name, request.Recipient.EmailAddress);
                return _success;
            }

            _logger.LogError("----- Error sending confirmation email to {Name} <{EmailAddress}>: {Errors}",
                request.Recipient.Name, request.Recipient.EmailAddress, string.Join(',', result.ErrorMessages));

            return _fail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "----- Error sending confirmation email to {Name} <{EmailAddress}>",
                request.Recipient.Name, request.Recipient.EmailAddress);

            return _fail;
        }
    }

    public override async Task<SendEmailResponse> SendPasswordResetEmail(SendPasswordResetEmailRequest request, ServerCallContext context)
    {
        _logger.LogInformation("----- Sending password reset email to {Name} <{EmailAddress}>", request.Recipient.Name, request.Recipient.EmailAddress);
        try
        {
            var email = _emailFactory.CreatePasswordResetEmail(
                new Models.Recipient(request.Recipient.EmailAddress, request.Recipient.Name),
                request.CallbackUrl,
                request.UrlExpirationAt.ToInstant());

            var result = await _sender.SendAsync(email).ConfigureAwait(false);

            if (result.Successful)
            {
                _logger.LogInformation("----- Password reset email to {Name} <{EmailAddress}> sent sucessfully", request.Recipient.Name, request.Recipient.EmailAddress);
                return _success;
            }

            _logger.LogError("----- Error sending password reset email to {Name} <{EmailAddress}>: {Errors}",
                request.Recipient.Name, request.Recipient.EmailAddress, string.Join(',', result.ErrorMessages));

            return _fail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "----- Error sending password reset email to {Name} <{EmailAddress}>",
                request.Recipient.Name, request.Recipient.EmailAddress);

            return _fail;
        }
    }

    public override Task<SendEmailResponse> SendNewPostEmail(SendNewPostEmailRequest request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }

    public override Task<SendEmailResponse> SendCustomEmail(SendCustomEmailRequest request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }
}