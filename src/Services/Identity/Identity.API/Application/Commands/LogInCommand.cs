using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record LogInCommand(NonEmptyString EmailAddress, NonEmptyString Password, bool IsPersistent) : IRequest<Unit>;