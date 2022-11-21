using System.Diagnostics.CodeAnalysis;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct NonNegativeInt
{
    public required int Value { get; init; }

    [SetsRequiredMembers]
    public NonNegativeInt(int value)
    {
        if (value < 0)
            throw new ArgumentException($"Value cannot be negative.");

        Value = value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not NonNegativeInt second)
            return false;

        return this.Value == second.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
    public static implicit operator NonNegativeInt(int value) => new(value);
    public static implicit operator int(NonNegativeInt value) => value.Value;
    public static NonNegativeInt operator ++(NonNegativeInt a) => new(a.Value + 1);
    public static bool operator >=(NonNegativeInt a, int b) => a.Value >= b;
    public static bool operator <=(NonNegativeInt a, int b) => a.Value <= b;
}
