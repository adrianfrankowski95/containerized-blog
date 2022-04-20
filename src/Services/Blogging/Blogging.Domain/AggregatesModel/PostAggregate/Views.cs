using Blog.Services.Blogging.Domain.Exceptions;
using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public class Views : ValueObject<Views>
{
    public int Count { get; private set; }

    public Views() => Count = 0;
    public Views(int count)
    {
        if (count < 0)
            throw new BloggingDomainException($"{nameof(Views)} {nameof(Count)} cannot be negative");

        Count = count;
    }

    public Views Increment() => new(++Count);

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return Count;
    }
}
