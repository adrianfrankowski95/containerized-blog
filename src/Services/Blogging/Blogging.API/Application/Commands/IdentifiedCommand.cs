using Blog.Services.Blogging.API.Application.Models;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record IdentifiedCommand<TRequest>(Guid Id, TRequest Command)
    : IRequest<ICommandResult> where TRequest : IRequest<ICommandResult>;