namespace Blog.Services.Identity.API.Core;

public class UsernameOptions
{
    public string AllowedCharacters { get; set; } = "aąbcćdeęfghijklłmnńopqrsśtuóvwxyzźż.-_1234567890";
    public int MinLength { get; set; } = 3;
    public int MaxLength { get; set; } = 32;
}