using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record PublishPostCommand(Guid PostId) : IRequest<Unit>;