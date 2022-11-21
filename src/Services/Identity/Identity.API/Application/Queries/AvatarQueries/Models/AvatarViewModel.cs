#nullable disable

namespace Blog.Services.Identity.API.Application.Queries.AvatarQueries.Models;

public record AvatarViewModel
{
    public byte[] ImageData { get; init; }
    public string Format { get; init; }
}
