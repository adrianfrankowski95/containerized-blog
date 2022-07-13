using Blog.Services.Emailing.API.Models;
using Blog.Services.Emailing.API.Templates.Identity;
using FluentEmail.Core;
using NodaTime;

namespace Blog.Services.Emailing.API.Factories;

public class FluentEmailFactory : IEmailFactory<IFluentEmail>
{
    private readonly IFluentEmail _email;

    public FluentEmailFactory(IFluentEmail email)
    {
        _email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public IFluentEmail CreateEmailConfirmationEmail(Recipient recipient, string callbackUrl, Instant urlExpirationAt)
    {
        return _email
            .To(recipient.EmailAddress, recipient.Name)
            .Subject("Email confirmation")
            .UsingTemplateFromFile("/Identity/EmailConfirmation",
                new EmailConfirmationModel(recipient.Name, callbackUrl, urlExpirationAt));
    }

    public IFluentEmail CreatePasswordResetEmail(Recipient recipient, string callbackUrl, Instant urlExpirationAt)
    {
        return _email
            .To(recipient.EmailAddress, recipient.Name)
            .Subject("Password reset")
            .UsingTemplateFromFile("/Identity/PasswordReset",
                new PasswordResetModel(recipient.Name, callbackUrl, urlExpirationAt));
    }

    //TODO: implement sending new post email and custom email, create controllers to expose the HTTP API
    public IFluentEmail CreateNewPostEmail(
        IEnumerable<Recipient> recipients,
        string postId,
        string postTitle,
        string postCategory,
        string postDescription,
        string authorId,
        string authorName,
        string headerImgUrl)
    {
        throw new NotImplementedException();
    }

    public IFluentEmail CreateCustomEmail(
        IEnumerable<Recipient> recipients,
        IEnumerable<Recipient> ccRecipients,
        IEnumerable<Recipient> bccRecipients,
        string title,
        string body,
        Priority priority,
        IEnumerable<Attachment>? attachments)
    {
        throw new NotImplementedException();
    }
}