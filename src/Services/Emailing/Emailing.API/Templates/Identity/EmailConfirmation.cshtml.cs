using NodaTime;

namespace Blog.Services.Emailing.API.Templates.Identity;

public record EmailConfirmationModel(string Username, string CallbackUrl, Instant UrlExpirationAt);