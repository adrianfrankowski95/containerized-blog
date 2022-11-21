using Blog.Services.Emailing.API.Extensions;

namespace Blog.Services.Emailing.API.Models;

public record SendEmailRequest
{
    public string Title { get; init; }
    public string Body { get; init; }
    public bool IsBodyHtml { get; init; }
    public Priority Priority { get; init; }
    public IEnumerable<Recipient> Recipients { get; init; }
    public IEnumerable<Recipient>? CcRecipients { get; init; }
    public IEnumerable<Recipient>? BccRecipients { get; init; }
    public IEnumerable<Attachment>? Attachments { get; init; }

    public SendEmailRequest(IEnumerable<Recipient> recipients,
        IEnumerable<Recipient>? ccRecipients,
        IEnumerable<Recipient>? bccRecipients,
        string title,
        string body,
        bool isBodyHtml,
        Priority priority,
        IEnumerable<Attachment>? attachments)
    {
        if (recipients.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(recipients));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentNullException(nameof(title));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentNullException(nameof(body));

        Title = title;
        Body = body;
        IsBodyHtml = isBodyHtml;

        Recipients = recipients;
        CcRecipients = ccRecipients;
        BccRecipients = bccRecipients;

        Priority = priority;

        Attachments = attachments;
    }
}