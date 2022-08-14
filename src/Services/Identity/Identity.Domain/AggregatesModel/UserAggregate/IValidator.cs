namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface IValidator<TItem>
{
    public IEnumerable<IRequirement<TItem>> Requirements { get; }
    public ValidationResult<TItem> Validate(TItem item);
}