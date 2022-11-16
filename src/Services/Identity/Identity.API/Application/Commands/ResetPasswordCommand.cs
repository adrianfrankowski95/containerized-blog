using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record ResetPasswordCommand(string EmailAddress, string NewPassword, string PasswordResetCode) : IRequest<Unit>;