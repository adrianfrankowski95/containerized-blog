using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record ConfirmEmailAddressCommand(string Username, string EmailConfirmationCode) : IRequest<Unit>;