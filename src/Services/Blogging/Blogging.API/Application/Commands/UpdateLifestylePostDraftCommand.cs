using Blog.Services.Blogging.API.Application.Commands.Models;
using MediatR;
namespace Blog.Services.Blogging.API.Application.Commands;

public record UpdateLifestylePostDraftCommand(
    Guid PostId,
    string HeaderImgUrl,
    IList<LifestylePostTranslationDTO> Translations) : IRequest<Unit>;