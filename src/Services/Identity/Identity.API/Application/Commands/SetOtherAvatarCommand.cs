using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record SetOtherAvatarCommand(EmailAddress EmailAddress, IFormFile ImageFile) : IRequest<Unit>;