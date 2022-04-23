namespace Blog.Services.Identity.API.Core;

public class IdentityOptions
{
    public UsernameOptions Username { get; set; }
    public PasswordOptions Password { get; set; }
    public EmailOptions Email { get; set; }
    public SecurityOptions Security { get; set; }
}

public class SecurityOptions
{
    public int MaxAllowedLoginAttempts { get; set; } = 10;
    public int AccountLockDurationMinutes { get; set; } = 15;
    public int PasswordResetCodeLength { get; set; } = 6;
}

public class PasswordOptions
{
    public bool RequireUpperCase { get; set; } = true;
    public bool RequireLowerCase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
    public int MinLength { get; set; } = 8;
    public int HashWorkFactor { get; set; } = 15;
}

public class EmailOptions
{
    public bool RequireUnique { get; set; } = true;
    public bool RequireConfirmed { get; set; } = true;
}

public class UsernameOptions
{
    public bool RequireUnique { get; set; } = true;
    public string AllowedCharacters { get; set; } = "aąbcćdeęfghijklłmnńopqrsśtuóvwxyzźż.-_1234567890";
    public int MinLength { get; set; } = 3;
    public int MaxLength { get; set; } = 32;
}