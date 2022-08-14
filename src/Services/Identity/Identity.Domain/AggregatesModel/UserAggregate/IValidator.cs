namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface IValidator<TItem, TResult> where TResult : ValidationResult<TItem>
{
    public IEnumerable<IRequirement<TItem>> Requirements { get; }
    public TResult Validate(TItem item);
}