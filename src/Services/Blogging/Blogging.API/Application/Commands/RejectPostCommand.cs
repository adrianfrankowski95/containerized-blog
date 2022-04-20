using Blog.Services.Blogging.API.Application.Models;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record RejectPostCommand(Guid PostId) : IRequest<ICommandResult>;