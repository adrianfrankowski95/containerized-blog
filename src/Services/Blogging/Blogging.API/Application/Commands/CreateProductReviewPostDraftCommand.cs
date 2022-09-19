using Blog.Services.Blogging.API.Application.Models;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record CreateProductReviewPostDraftCommand(
    string HeaderImgUrl,

    string ProductName,
    string ProductWebsiteUrl,
    int Rating,

    IList<ProductReviewPostTranslationDTO> Translations) : IRequest<Unit>;
