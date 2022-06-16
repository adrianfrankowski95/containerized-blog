using FluentEmail.Core.Interfaces;
using Grpc.Core;

namespace Blog.Services.Emailing.API.Grpc;

public class EmailingService : GrpcEmailingService.GrpcEmailingServiceBase
{
    private readonly ILogger<EmailingService> _logger;
    private readonly ISender _sender;

    public EmailingService(
        ISender sender,
        ILogger<EmailingService> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override Task<SendEmailResponse> SendEmailConfirmationEmail(SendEmailConfirmationEmailRequest request, ServerCallContext context)
    {
        return base.SendEmailConfirmationEmail(request, context);
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