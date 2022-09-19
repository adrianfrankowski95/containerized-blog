using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record RegisterCommand(
    string Username,
    string FirstName,
    string LastName,
    string Gender,
    bool ReceiveAdditionalEmails,
    string EmailAddress,
    string Password) : IRequest<Unit>;