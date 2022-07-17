using System;
using System.IO;

namespace Blog.Services.Emailing.API.Models;

public record Attachment
{
    public string Filename { get; init; }
    public Stream Data { get; init; }
    public string ContentType { get; init; }
    public string ContentId { get; init; }
    public Attachment(string filename, Stream data, string contentType, string contentId)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentNullException(nameof(filename));

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentNullException(nameof(contentType));

        if (string.IsNullOrWhiteSpace(contentId))
            throw new ArgumentNullException(nameof(contentId));

        Filename = filename;
        Data = data;
        ContentType = contentType;
        ContentId = contentId;
    }
}