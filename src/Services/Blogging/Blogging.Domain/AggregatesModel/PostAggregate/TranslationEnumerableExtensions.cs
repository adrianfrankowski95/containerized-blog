using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public static class TranslationEnumerableExtensions
{
    public static IEnumerable<Language> GetLanguages<T>(this IEnumerable<T> @this) where T : ITranslated
    {
        foreach (var translation in @this)
            yield return translation.Language;
    }
    public static bool ContainsDuplicatedLanguages<T>(this IEnumerable<T> @this) where T : ITranslated
    {
        var languages = @this.GetLanguages();
        var distinctLaguages = languages.Distinct();

        return languages.Count() != distinctLaguages.Count();
    }

    public static bool ContainsDefaultLanguage<T>(this IEnumerable<T> @this) where T : ITranslated
    {
        return @this.Any(x => x.Language.Equals(Language.GetDefault()));
    }

    public static bool MatchPostId<T>(this IEnumerable<T> @this, PostId postId) where T : PostTranslationBase
    {
        return !@this.Any(x => x?.PostId?.Equals(postId) ?? throw new BloggingDomainException("Post must not have a null ID"));
    }
}
