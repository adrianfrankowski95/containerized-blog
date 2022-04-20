using System.Reflection;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.SeedWork;

public abstract class Enumeration
{
    public string Name { get; protected set; }
    public int Value { get; protected set; }
    protected Enumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public override string ToString() => Name;
    public static IEnumerable<T> GetAllEnums<T>() where T : Enumeration =>
                typeof(T).GetFields(BindingFlags.Public |
                                    BindingFlags.Static |
                                    BindingFlags.DeclaredOnly)
                        .Select(f => f.GetValue(null))
                        .Cast<T>();

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj is not Enumeration o)
            return false;

        var type = GetType();

        bool equalType = type.Equals(o.GetType());
        bool equalValue = Value.Equals(o.Value);
        bool equalName = string.Equals(Name, o.Name, StringComparison.Ordinal);

        if (equalValue && !equalName)
            throw new BloggingDomainException($"Name ambiguity for type {type.Name}");

        if (equalName && !equalValue)
            throw new BloggingDomainException($"Value ambiguity for type {type.Name}");

        return equalType && equalValue && equalName;
    }

    public override int GetHashCode() => Value;
}
