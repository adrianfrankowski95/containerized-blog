namespace Blog.Services.Emailing.API.Models;

public record Attachment(string Filename, Stream Data, string ContentType, string ContentId);