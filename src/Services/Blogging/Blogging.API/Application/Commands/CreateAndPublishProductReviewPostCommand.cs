using Blog.Services.Blogging.API.Application.Commands.Models;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record CreateAndPublishProductReviewPostCommand(
    string HeaderImgUrl,

    string ProductName,
    string ProductWebsiteUrl,
    int Rating,

    IList<ProductReviewPostTranslationDTO> Translations) : IRequest<Unit>;
