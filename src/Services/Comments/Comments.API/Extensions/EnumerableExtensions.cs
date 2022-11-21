namespace Blog.Services.Comments.API.Extensions;

public static class EnumerableExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        if (source is null)
            return true;

        if (source is ICollection<T> list)
            return list.Count > 0;

        if (source is Array array)
            return array.Length > 0;

        return source.Any();
    }
}