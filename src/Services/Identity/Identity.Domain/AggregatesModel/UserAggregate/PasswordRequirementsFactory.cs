namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public static class PasswordRequirementsFactory
{
    public static List<IRequirement<Password>> DefaultSet()
        => new List<IRequirement<Password>>
        {
            new PasswordMustBe8CharactersLong(),
            new PasswordMustContainDigit(),
            new PasswordMustContainLowercase(),
            new PasswordMustContainNonAlphanumeric(),
            new PasswordMustContainUppercase()
        };
}

public class PasswordMustBe8CharactersLong : IRequirement<Password>
{
    public RequirementMessage<Password> Message => "Password must be at least 8 characters long.";
    public bool IsSatisfiedBy(Password password) => password.Length >= 8;
}

public class PasswordMustContainDigit : IRequirement<Password>
{
    public RequirementMessage<Password> Message => "Password must contain at least one digit.";
    public bool IsSatisfiedBy(Password password) => password.Any(c => IsDigit(c));
    private static bool IsDigit(char c) => c is >= '0' and <= '9';
}

public class PasswordMustContainUppercase : IRequirement<Password>
{
    public RequirementMessage<Password> Message => "Password must contain at least one uppercase character.";
    public bool IsSatisfiedBy(Password password) => password.Any(c => IsUppercase(c));
    private static bool IsUppercase(char c) => c is >= 'A' and <= 'Z';
}

public class PasswordMustContainLowercase : IRequirement<Password>
{
    public RequirementMessage<Password> Message => "Password must contain at least one lowercase character.";
    public bool IsSatisfiedBy(Password password) => password.Any(c => IsLowercase(c));
    private static bool IsLowercase(char c) => c is >= 'a' and <= 'z';
}

public class PasswordMustContainNonAlphanumeric : IRequirement<Password>
{
    public RequirementMessage<Password> Message => "Password must contain at least one non-alphanumeric character.";
    public bool IsSatisfiedBy(Password password) => password.Any(c => IsNonAlphanumeric(c));
    private static bool IsDigit(char c) => c is >= '0' and <= '9';
    private static bool IsUppercase(char c) => c is >= 'A' and <= 'Z';
    private static bool IsLowercase(char c) => c is >= 'a' and <= 'z';
    private static bool IsNonAlphanumeric(char c) => !IsDigit(c) && !IsUppercase(c) && !IsLowercase(c);
}