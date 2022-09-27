using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record LogInCommand(EmailAddress EmailAddress, NonEmptyString Password) : IRequest<Unit>;