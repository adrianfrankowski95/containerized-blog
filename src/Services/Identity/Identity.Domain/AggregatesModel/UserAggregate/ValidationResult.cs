using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class ValidationResult<TItem> : ValueObject<ValidationResult<TItem>>
{
    private static ValidationResult<TItem> _success => new();
    public static ValidationResult<TItem> Success() => _success;
    public bool IsSuccess { get; }
    public string? Errors { get; }

    private ValidationResult()
    {
        IsSuccess = true;
    }

    private ValidationResult(IEnumerable<RequirementMessage<TItem>> errors)
    {
        IsSuccess = false;
        Errors = string.Join<RequirementMessage<TItem>>(Environment.NewLine, errors);
    }

    public static ValidationResult<TItem> Fail(IEnumerable<RequirementMessage<TItem>> errors) => new(errors);

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return IsSuccess;
        yield return Errors;
    }
}