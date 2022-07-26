using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record SetPostToDraftCommand(Guid PostId) : IRequest<Unit>;