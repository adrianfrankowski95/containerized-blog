using Blog.Services.Blogging.Domain.Exceptions;
using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public class Comments : ValueObject<Comments>
{
    public int Count { get; private set; }

    public Comments() => Count = 0;
    public Comments(int count)
    {
        if (count < 0)
            throw new BloggingDomainException($"{nameof(Comments)} {nameof(Count)} cannot be negative");

        Count = count;
    }

    public Comments Increment() => new(++Count);
    public Comments Decrement() => new(--Count);

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Count;
    }
}
