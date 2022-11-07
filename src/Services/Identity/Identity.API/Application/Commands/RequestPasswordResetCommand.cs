using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record RequestPasswordResetCommand(NonEmptyString EmailAddress) : IRequest<Unit>;