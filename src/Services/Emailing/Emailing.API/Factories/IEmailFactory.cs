using Blog.Services.Emailing.API.Models;
using NodaTime;

namespace Blog.Services.Emailing.API.Factories;

public interface IEmailFactory<TEmail>
{
    public TEmail CreateEmailConfirmationEmail(Recipient recipient, string callbackUrl, Instant urlExpirationAt);
    public TEmail CreatePasswordResetEmail(Recipient recipient, string callbackUrl, Instant urlExpirationAt);
    public TEmail CreateNewPostEmail(
        IEnumerable<Recipient> recipients,
        string postId,
        string postTitle,
        string postCategory,
        string postDescription,
        string authorId,
        string authorName,
        string headerImgUrl);

    public TEmail CreateCustomEmail(
        IEnumerable<Recipient> recipients,
        IEnumerable<Recipient> ccRecipients,
        IEnumerable<Recipient> bccRecipients,
        string title,
        string body,
        Priority priority,
        IEnumerable<Attachment>? attachments);
}