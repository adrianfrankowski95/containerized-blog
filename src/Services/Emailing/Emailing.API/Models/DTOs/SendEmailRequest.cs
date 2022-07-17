using System.Collections.Generic;
using System.IO;

namespace Blog.Services.Emailing.API.Models.DTOs;

public record SendEmailRequest(
    IEnumerable<Recipient> Recipients,
    IEnumerable<Recipient>? CcRecipients,
    IEnumerable<Recipient>? BccRecipients,
    string Title,
    string Body,
    string Priority,
    IEnumerable<Attachment>? Attachments
);

public record Recipient(string EmailAddress, string Name);
public record Attachment(string Filename, Stream Data, string ContentType, string ContentId);