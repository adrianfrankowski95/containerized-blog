namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface IRequirement<TItem>
{
    public bool IsSatisfiedBy(TItem item);
    public RequirementMessage<TItem> Message { get; }
}