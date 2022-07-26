using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public record IdentifiedCommand<TRequest>(Guid Id, TRequest Command)
    : IRequest<Unit> where TRequest : IRequest<Unit>;