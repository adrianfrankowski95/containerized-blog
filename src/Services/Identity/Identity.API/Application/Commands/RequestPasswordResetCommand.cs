using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record RequestPasswordResetCommand(string EmailAddress) : IRequest<Unit>;