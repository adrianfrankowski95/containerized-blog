using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Identity.API.Core;

public class UsernameOptions
{
    [Required]
    [StringLength(int.MaxValue, MinimumLength = 1)]
    public string AllowedCharacters { get; set; } = "aąbcćdeęfghijklłmnńopqrsśtuóvwxyzźż.-_1234567890";

    [Required]
    [Range(0, int.MaxValue)]
    public int MinLength { get; set; } = 3;

    [Required]
    [Range(0, int.MaxValue)]
    public int MaxLength { get; set; } = 32;
}