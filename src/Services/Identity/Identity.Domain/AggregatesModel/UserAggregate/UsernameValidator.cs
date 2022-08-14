namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class UsernameValidator : IValidator<Username>
{
    public IEnumerable<IRequirement<Username>> Requirements { get; }

    public UsernameValidator(IEnumerable<IRequirement<Username>> requirements)
    {
        if (requirements is null || !requirements.Any())
            throw new ArgumentNullException(nameof(requirements));

        Requirements = requirements;
    }

    public ValidationResult<Username> Validate(Username username)
    {
        var violatedRequirements = Requirements.Where(r => !r.IsSatisfiedBy(username));

        return violatedRequirements.Any()
            ? ValidationResult<Username>.Fail(violatedRequirements.Select(m => m.Message))
            : ValidationResult<Username>.Success();
    }
}