using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using MediatR;

namespace Blog.Services.Identity.API.Application.Commands;

public record SetProfilePictureCommand(Guid UserId, IFormFile ImageData) : IRequest<Unit>;