using NodaTime;

namespace Blog.Services.Emailing.API.Templates.Identity;

public record PasswordResetModel(string Username, string CallbackUrl, Instant UrlExpirationAt);