using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Blog.Services.Blogging.Infrastructure.Comparers;

public class EnumerationComparer<T> : ValueComparer<List<T>> where T : Enumeration
{
    public EnumerationComparer() : base(
        (e1, e2) =>
            e1.IsNullOrEmpty() || e2.IsNullOrEmpty()
            ? e1.IsNullOrEmpty() && e2.IsNullOrEmpty()
            : e1!.SequenceEqual(e2!),
        e => e.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
        e => (List<T>)e)
    {

    }
}
