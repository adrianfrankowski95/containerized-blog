using Blog.Services.Blogging.API.Application.Commands.Models;
using Blog.Services.Blogging.API.Application.Models;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record CreateAndSubmitRestaurantReviewPostCommand(
    string HeaderImgUrl,

    string RestaurantName,
    string RestaurantWebsiteUrl,
    int Rating,

    IList<RestaurantReviewPostTranslationDTO> Translations) : IRequest<ICommandResult>;