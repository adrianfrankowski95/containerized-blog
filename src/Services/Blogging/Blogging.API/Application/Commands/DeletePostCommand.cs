using Blog.Services.Blogging.API.Application.Models;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record DeletePostCommand(Guid PostId) : IRequest<ICommandResult>;