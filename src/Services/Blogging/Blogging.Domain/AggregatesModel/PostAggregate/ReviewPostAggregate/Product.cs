namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;

public class Product : ReviewItem
{
    //ef core
    private Product() { }
    public Product(string name, string websiteUrl) : base(name, websiteUrl) { }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        return base.GetEqualityCheckAttributes();
    }

    public override void Validate()
    {
        base.Validate();
    }
}
