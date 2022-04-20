using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record ApprovePostCommand(Guid PostId) : IRequest<Unit>;