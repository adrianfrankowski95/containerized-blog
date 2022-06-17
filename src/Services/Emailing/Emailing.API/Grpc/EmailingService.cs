using Blog.Services.Emailing.API.Templates.Identity;
using FluentEmail.Core;
using Grpc.Core;
using NodaTime.Serialization.Protobuf;

namespace Blog.Services.Emailing.API.Grpc;

public class EmailingService : GrpcEmailingService.GrpcEmailingServiceBase
{
    private readonly ILogger<EmailingService> _logger;
    private readonly IFluentEmail _sender;

    private readonly static SendEmailResponse _success = new() { Success = true };
    private readonly static SendEmailResponse _fail = new() { Success = false };

    public EmailingService(
        IFluentEmail sender,
        ILogger<EmailingService> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<SendEmailResponse> SendEmailConfirmationEmail(SendEmailConfirmationEmailRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _sender
                    .To(request.Recipient.EmailAddress, request.Recipient.Name)
                    .Subject("Email confirmation")
                    .UsingTemplateFromFile("/Identity/EmailConfirmation",
                        new EmailConfirmationModel(
                            request.Recipient.Name,
                            request.CallbackUrl,
                            request.UrlExpirationAt.ToInstant()))
                    .SendAsync().ConfigureAwait(false);

            if (result.Successful)
                return _success;

            return _fail;
        }
        catch (Exception ex)
        {

            return _fail;
        }
    }

    public override Task<SendEmailResponse> SendPasswordResetEmail(SendPasswordResetEmailRequest request, ServerCallContext context)
    {
        return base.SendPasswordResetEmail(request, context);
    }

    public override Task<SendEmailResponse> SendNewPostEmail(SendNewPostEmailRequest request, ServerCallContext context)
    {
        return base.SendNewPostEmail(request, context);
    }

    public override Task<SendEmailResponse> SendCustomEmail(SendCustomEmailRequest request, ServerCallContext context)
    {
        return base.SendCustomEmail(request, context);
    }
}