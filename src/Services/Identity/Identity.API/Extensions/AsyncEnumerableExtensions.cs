namespace Blog.Services.Identity.API.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var list = new List<T>();

        await foreach (var element in source.ConfigureAwait(false))
            list.Add(element);

        return list;
    }
}