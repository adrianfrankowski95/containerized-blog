using Blog.Services.Blogging.API.Application.Models;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record UpdateAndSubmitProductReviewPostCommand(
    Guid PostId,
    string HeaderImgUrl,

    string ProductName,
    string ProductWebsiteUrl,
    int Rating,

    IList<ProductReviewPostTranslationDTO> Translations) : IRequest<Unit>;
