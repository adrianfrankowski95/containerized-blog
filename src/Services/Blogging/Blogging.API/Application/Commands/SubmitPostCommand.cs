using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record SubmitPostCommand(Guid PostId) : IRequest<Unit>;