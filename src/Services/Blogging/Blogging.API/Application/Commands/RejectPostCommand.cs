using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record RejectPostCommand(Guid PostId) : IRequest<Unit>;