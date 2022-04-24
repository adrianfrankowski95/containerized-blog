namespace Blog.Services.Identity.API.Models;

public record NonEmptyString
{
    public string Value { get; init; }

    public NonEmptyString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));

        Value = value;
    }

    public override string ToString() => Value;
    public static implicit operator string(NonEmptyString a) => a.Value;
    public static implicit operator NonEmptyString(string a) => new(a);
}
