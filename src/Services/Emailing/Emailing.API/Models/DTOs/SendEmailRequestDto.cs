namespace Blog.Services.Emailing.API.Models.DTOs;

public record SendEmailRequestDto(
    IEnumerable<RecipientDto> Recipients,
    IEnumerable<RecipientDto>? CcRecipients,
    IEnumerable<RecipientDto>? BccRecipients,
    string Title,
    string Body,
    bool IsBodyHtml,
    string Priority,
    IEnumerable<AttachmentDto>? Attachments
);

public record RecipientDto(string EmailAddress, string Name);
public record AttachmentDto(string Filename, Stream Data, string ContentType, string ContentId);