namespace Blog.Services.Blogging.API.Application.Commands.Models;

public record ProductReviewPostTranslationDTO(
    string Language,
    string Title,
    string Content,
    string Description,
    IList<Guid> TagIds);