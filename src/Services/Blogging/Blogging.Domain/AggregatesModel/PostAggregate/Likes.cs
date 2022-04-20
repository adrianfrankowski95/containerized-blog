using Blog.Services.Blogging.Domain.Exceptions;
using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public class Likes : ValueObject<Likes>
{
    public int Count { get; private set; }

    public Likes() => Count = 0;
    public Likes(int count)
    {
        if (count < 0)
            throw new BloggingDomainException($"{nameof(Likes)} {nameof(Count)} cannot be negative");

        Count = count;
    }

    public Likes AddLike() => new(++Count);
    public Likes AddLikeBy(User user) => new(++Count);

    public Likes RemoveLike() => new(--Count);
    public Likes RemoveLikeBy(User user) => new(--Count);


    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return Count;
    }
}

