using Blog.Services.Blogging.Domain.Exceptions;
using Blog.Services.Blogging.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public abstract class PostBase : Entity<PostId>, IAggregateRoot, IValidatable
{
    protected readonly HashSet<PostTranslationBase> _translations;
    public IReadOnlySet<PostTranslationBase> Translations => _translations;
    public PostCategory Category { get; protected set; }
    public PostType Type { get; protected set; }
    public PostStatus Status { get; private set; }
    public User Author { get; }
    public Instant CreatedAt { get; }
    public User? Editor { get; private set; }
    public Instant? EditedAt { get; private set; }
    public Views Views { get; }
    public Likes Likes { get; }
    public Comments Comments { get; }
    public string HeaderImgUrl { get; private set; }

    // EF Core
    protected PostBase() { }

    protected PostBase(
        User author,
        PostCategory category,
        PostType type,
        IEnumerable<PostTranslationBase> translations,
        string headerImgUrl)
    {
        if (!author.Role.Equals(UserRole.Author) && !author.Role.Equals(UserRole.Administrator))
            throw new BloggingDomainException($"Only {nameof(UserRole.Author)} and {nameof(UserRole.Administrator)} can create Post");

        if (EditedAt is not null)
            throw new BloggingDomainException($"New Post cannot be edited before");

        if (Editor is not null)
            throw new BloggingDomainException($"New Post cannot have an editor yet");

        if (category is null)
            throw new BloggingDomainException($"{nameof(Category)} must not be null");

        if (type is null)
            throw new BloggingDomainException($"{nameof(Type)} must not be null");

        if (translations is null)
            throw new BloggingDomainException($"{nameof(Translations)} cannot be null");

        if (translations.ContainsDuplicatedLanguages())
            throw new BloggingDomainException($"{nameof(Translations)} cannot contain duplicated languages");

        Id = new();

        Author = author;
        Category = category;
        Type = type;
        Status = PostStatus.Draft;
        CreatedAt = SysTime.Now;
        HeaderImgUrl = headerImgUrl;
        Views = new();
        Likes = new();
        Comments = new();

        _translations = new(translations.Count());

        foreach (var translation in translations)
            _translations.Add(translation.AssignPost(Id));

        //TODO: new PostCreatedEvent - CorrelationId
    }

    protected virtual bool UpdateBy(
        User editor,
        IEnumerable<PostTranslationBase> newTranslations,
        string newHeaderImgUrl)
    {
        if (!editor.Role.Equals(UserRole.Author) && !editor.Role.Equals(UserRole.Administrator))
            throw new BloggingDomainException($"Only {nameof(UserRole.Author)} and {nameof(UserRole.Administrator)} can edit Post");

        if (editor.Role.Equals(UserRole.Author))
        {
            if (!editor.Id.Equals(Author.Id))
                throw new BloggingDomainException($"Only Author can edit his Post");

            if (Status.Equals(PostStatus.Deleted))
                throw new BloggingDomainException($"{nameof(PostStatus.Deleted)} Post cannot be edited by {nameof(UserRole.Author)}");
        }

        if (newTranslations.ContainsDuplicatedLanguages())
            throw new BloggingDomainException($"Translations cannot contain duplicated languages");

        bool isChanged = false;

        if (!string.Equals(HeaderImgUrl, newHeaderImgUrl, StringComparison.Ordinal))
        {
            HeaderImgUrl = newHeaderImgUrl;
            isChanged = true;
        }

        if (!_translations.SetEquals(newTranslations))
        {
            _translations.Clear();

            foreach (var translation in newTranslations)
            {
                _translations.Add(translation.AssignPost(Id));
            }

            isChanged = true;
        }

        if (isChanged)
        {
            Editor = editor;
            EditedAt = SysTime.Now;
        }

        return isChanged;
    }

    public virtual void ToDraftBy(User user)
    {
        if (!user.Role.Equals(UserRole.Author) && !user.Role.Equals(UserRole.Administrator))
            throw new BloggingDomainException($"Only {nameof(UserRole.Author)} and {nameof(UserRole.Administrator)} can set Post back to draft");

        if (user.Role.Equals(UserRole.Author))
        {
            if (!user.Id.Equals(Author.Id))
                throw new BloggingDomainException($"Only Author can set his Post back to draft");

            if (Status.Equals(PostStatus.Deleted))
                throw new BloggingDomainException($"{nameof(PostStatus.Deleted)} Post cannot be set back to draft by {nameof(UserRole.Author)}");
        }

        Status = PostStatus.Draft;
    }

    public virtual void PublishBy(User user)
    {
        if (!user.Role.Equals(UserRole.Administrator))
            throw new BloggingDomainException($"Only {nameof(UserRole.Administrator)} can publish Post");

        Validate();

        Status = PostStatus.Published;

        //TODO: new PostStatusChangedToPublishedEvent - CorrelationId
    }

    public virtual void SubmitBy(User user)
    {
        if (!user.Role.Equals(UserRole.Author))
            throw new BloggingDomainException($"Only {nameof(UserRole.Author)} can submit Post");

        if (!user.Id.Equals(Author.Id))
            throw new BloggingDomainException($"Only Author can submit Post");

        if (!Status.Equals(PostStatus.Draft) &&
            !Status.Equals(PostStatus.Submitted) &&
            !Status.Equals(PostStatus.Rejected))
            throw new BloggingDomainException($"Only {nameof(PostStatus.Submitted)}, {nameof(PostStatus.Draft)} and {nameof(PostStatus.Rejected)} Post can be submitted");

        Validate();

        Status = PostStatus.Submitted;

        //TODO: new PostStatusChangedToAwaitingApprovalEvent - CorrelationId
    }

    public virtual void ApproveBy(User user)
    {
        if (!user.Role.Equals(UserRole.Administrator))
            throw new BloggingDomainException($"Only {nameof(UserRole.Administrator)} can approve Post");

        if (!Status.Equals(PostStatus.Submitted))
            throw new BloggingDomainException($"Only {nameof(PostStatus.Submitted)} Post can be approved");

        Validate();

        Status = PostStatus.Published;

        //TODO: new PostStatusChangedToPublishedEvent - CorrelationId
    }

    public virtual void RejectBy(User user)
    {
        if (!user.Role.Equals(UserRole.Administrator))
            throw new BloggingDomainException($"Only {nameof(UserRole.Administrator)} can reject Post");

        if (!Status.Equals(PostStatus.Submitted))
            throw new BloggingDomainException($"Only {nameof(PostStatus.Submitted)} Post can be rejected");

        Status = PostStatus.Rejected;
    }

    public virtual void DeleteBy(User user)
    {
        if (!user.Role.Equals(UserRole.Administrator))
            throw new BloggingDomainException($"Only {nameof(UserRole.Administrator)} can delete Post");

        Status = PostStatus.Deleted;

        //TODO: new PostStatusChangedToAwaitingApprovalEvent - CorrelationId
    }

    public virtual void Validate()
    {
        if (Id is null)
            throw new BloggingDomainException($"{nameof(Id)} cannot be null");

        if (Id.IsEmpty)
            throw new BloggingDomainException($"{nameof(Id)} cannot be empty");

        if (Category is null)
            throw new BloggingDomainException($"{nameof(Category)} cannot be null");

        if (Author.Id is null)
            throw new BloggingDomainException($"{nameof(Author.Id)} cannot be null");

        if (Author.Id.IsEmpty)
            throw new BloggingDomainException($"{nameof(Author.Id)} cannot be empty");

        if (CreatedAt == default)
            throw new BloggingDomainException($"{nameof(CreatedAt)} cannot be default");

        if (CreatedAt == Instant.MinValue)
            throw new BloggingDomainException($"{nameof(CreatedAt)} cannot be a minimum value");

        if (Status is null)
            throw new BloggingDomainException($"{nameof(Status)} cannot be null");

        if (Views is null)
            throw new BloggingDomainException($"{nameof(Views)} cannot be null");

        if (Views.Count < 0)
            throw new BloggingDomainException($"{nameof(Views.Count)} cannot be negative");

        if (Comments is null)
            throw new BloggingDomainException($"{nameof(Views)} cannot be null");

        if (Comments.Count < 0)
            throw new BloggingDomainException($"{nameof(Views.Count)} cannot be negative");

        if (Translations is null)
            throw new BloggingDomainException($"{nameof(Translations)} cannot be null");

        if (Translations.Count == 0)
            throw new BloggingDomainException($"{nameof(Translations)} canot be empty");

        if (!Translations.MatchPostId(Id))
            throw new BloggingDomainException($"Not all {nameof(Translations)} match the Post Id");

        if (!Translations.ContainsDefaultLanguage())
            throw new BloggingDomainException($"{nameof(Translations)} must contain a default language");

        if (Translations.ContainsDuplicatedLanguages())
            throw new BloggingDomainException($"{nameof(Translations)} cannot contain duplicated languages");

        foreach (var translation in _translations)
            translation.Validate();
    }
}

public class PostId : ValueObject<PostId>
{
    public Guid Value { get; }

    public PostId() => Value = Guid.NewGuid();
    public PostId(Guid postId) => Value = postId;

    public bool IsEmpty => Value.Equals(Guid.Empty);

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Value;
    }
}