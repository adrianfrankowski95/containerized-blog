namespace Blog.Services.Identity.API.Models;

public record NonNegativeInt
{
    public int Value { get; init; }

    public NonNegativeInt(int value)
    {
        if (value < 0)
            throw new ArgumentException($"Value cannot be negative");

        Value = value;
    }

    public static NonNegativeInt operator ++(NonNegativeInt a) => new(a.Value + 1);
    public static bool operator >=(NonNegativeInt a, int b) => a.Value >= b;
    public static bool operator <=(NonNegativeInt a, int b) => a.Value <= b;
    public static bool operator ==(NonNegativeInt a, int b) => a.Value == b;
    public static bool operator !=(NonNegativeInt a, int b) => a.Value != b;
    public static implicit operator int(NonNegativeInt a) => a.Value;
    public static implicit operator NonNegativeInt(int a) => new(a);
}
