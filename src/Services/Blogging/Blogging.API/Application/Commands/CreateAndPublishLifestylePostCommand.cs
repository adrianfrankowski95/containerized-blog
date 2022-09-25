using Blog.Services.Blogging.API.Application.Commands.Models;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record CreateAndPublishLifestylePostCommand(
    string HeaderImgUrl,
    IList<LifestylePostTranslationDTO> Translations) : IRequest<Unit>;

