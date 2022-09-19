using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record LogInCommand(string EmailAddress, string Password) : IRequest<Unit>;