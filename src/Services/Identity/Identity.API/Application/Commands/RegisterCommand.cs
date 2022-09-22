using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record RegisterCommand(
    NonEmptyString Username,
    NonEmptyString FirstName,
    NonEmptyString LastName,
    NonEmptyString Gender,
    bool ReceiveAdditionalEmails,
    EmailAddress EmailAddress,
    Password Password) : IRequest<Unit>;