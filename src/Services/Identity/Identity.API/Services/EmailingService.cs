using System.ComponentModel.DataAnnotations;
using Blog.Services.Emailing.API.Grpc;
using NodaTime;
using NodaTime.Serialization.Protobuf;

namespace Blog.Services.Identity.API.Services;

public class EmailingService : IEmailingService
{
    private readonly GrpcEmailingService.GrpcEmailingServiceClient _client;
    private readonly ILogger<EmailingService> _logger;

    public EmailingService(GrpcEmailingService.GrpcEmailingServiceClient client, ILogger<EmailingService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SendEmailConfirmationEmailAsync(string username, string emailAddress, string callbackUrl, Instant urlExpirationAt)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username));

        if (string.IsNullOrWhiteSpace(emailAddress))
            throw new ArgumentNullException(nameof(emailAddress));

        if (!new EmailAddressAttribute().IsValid(emailAddress))
            throw new ArgumentException("Invalid email address format");

        if (string.IsNullOrWhiteSpace(callbackUrl))
            throw new ArgumentNullException(nameof(callbackUrl));

        if (urlExpirationAt == default)
            throw new ArgumentException("Invalid callback URL expiration date");

        _logger.LogInformation("----- Sending grpc request Send Email Confirmation Email to emailing service, arguments: "
            + "{Username}, {EmailAddress}, {CallbackUrl}, {UrlExpiration}", username, emailAddress, callbackUrl, urlExpirationAt);

        var response = await _client.SendEmailConfirmationEmailAsync(new SendEmailConfirmationEmailRequest
        {
            CallbackUrl = callbackUrl,
            Recipient = new Recipient { Name = username, EmailAddress = emailAddress },
            UrlExpirationAt = urlExpirationAt.ToTimestamp(),
        }).ConfigureAwait(false);


        _logger.LogInformation("----- Received grpc request Send Email Confirmation Email response: {Response}", response.Success);

        return response.Success;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string username, string emailAddress, string callbackUrl, Instant urlExpirationAt)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username));

        if (string.IsNullOrWhiteSpace(emailAddress))
            throw new ArgumentNullException(nameof(emailAddress));

        if (!new EmailAddressAttribute().IsValid(emailAddress))
            throw new ArgumentException("Invalid email address format");

        if (string.IsNullOrWhiteSpace(callbackUrl))
            throw new ArgumentNullException(nameof(callbackUrl));

        if (urlExpirationAt == default)
            throw new ArgumentException("Invalid callback URL expiration date");

        _logger.LogInformation("----- Sending grpc request Send Password Reset Email to emailing service, arguments: "
            + "{Username}, {EmailAddress}, {CallbackUrl}, {UrlExpiration}", username, emailAddress, callbackUrl, urlExpirationAt);

        var response = await _client.SendPasswordResetEmailAsync(new SendPasswordResetEmailRequest
        {
            CallbackUrl = callbackUrl,
            Recipient = new Recipient { Name = username, EmailAddress = emailAddress },
            UrlExpirationAt = urlExpirationAt.ToTimestamp(),
        }).ConfigureAwait(false);

        _logger.LogInformation("----- Received grpc request Send Password Reset Email response: {Response}", response.Success);

        return response.Success;
    }
}