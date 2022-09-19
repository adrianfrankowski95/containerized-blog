using Blog.Services.Blogging.API.Application.Models;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record CreateRestaurantReviewPostDraftCommand(
    string HeaderImgUrl,

    string RestaurantName,
    string RestaurantWebsiteUrl,
    int Rating,

    IList<RestaurantReviewPostTranslationDTO> Translations) : IRequest<Unit>;