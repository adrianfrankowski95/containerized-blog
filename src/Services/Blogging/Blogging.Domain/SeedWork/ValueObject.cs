namespace Blog.Services.Blogging.Domain.SeedWork;

public abstract class ValueObject<T> where T : ValueObject<T>
{
    protected abstract IEnumerable<object> GetEqualityCheckAttributes();

    public override bool Equals(object? second) => Equals(second as T);

    public virtual bool Equals(T? second)
    {
        if (second is null)
            return false;

        return GetEqualityCheckAttributes().SequenceEqual(second.GetEqualityCheckAttributes());
    }

    public override int GetHashCode() =>
            GetEqualityCheckAttributes()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);

    public T Copy() => (T)MemberwiseClone();
}