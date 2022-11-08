namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;

public class Restaurant : ReviewItem
{
    // EF Core
    private Restaurant() { }
    public Restaurant(
        string name,
        string websiteUrl)
        : base(name, websiteUrl)
    {

    }
    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        return base.GetEqualityCheckAttributes();
    }

    public override void Validate()
    {
        base.Validate();
    }
}
