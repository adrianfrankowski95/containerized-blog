namespace Blog.Services.Identity.API.Models;

public record NonNegativeNumber
{
    public int Value { get; init; }

    public NonNegativeNumber(int value)
    {
        if (value < 0)
            throw new ArgumentException($"Value cannot be negative");

        Value = value;
    }

    public static NonNegativeNumber operator ++(NonNegativeNumber a) => new(a.Value + 1);
    public static bool operator >=(NonNegativeNumber a, int b) => a.Value >= b;
    public static bool operator <=(NonNegativeNumber a, int b) => a.Value <= b;
    public static bool operator ==(NonNegativeNumber a, int b) => a.Value == b;
    public static bool operator !=(NonNegativeNumber a, int b) => a.Value != b;
}
