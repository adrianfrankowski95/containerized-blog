namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface IValidator<TItem, TResult> where TResult: ValidationResult<TItem>
{
    public IReadOnlyList<IRequirement<TItem>> Requirements { get; }
    public TResult Validate(TItem item);
}