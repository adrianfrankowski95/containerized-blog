using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record SetOwnAvatarCommand(IFormFile ImageFile) : IRequest<Unit>;