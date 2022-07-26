using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record DeletePostCommand(Guid PostId) : IRequest<Unit>;