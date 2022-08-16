namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class UsernameValidator : IValidator<Username>
{
    public IEnumerable<IRequirement<Username>>? Requirements { get; }

    public UsernameValidator(IEnumerable<IRequirement<Username>> requirements)
    {
        Requirements = requirements;
    }

    public ValidationResult<Username> Validate(Username username)
    {
        if(Requirements is null || !Requirements.Any())
            return ValidationResult<Username>.Success;

        var violatedRequirements = Requirements.Where(r => !r.IsSatisfiedBy(username));

        return violatedRequirements.Any()
            ? ValidationResult<Username>.Fail(violatedRequirements.Select(m => m.Message))
            : ValidationResult<Username>.Success;
    }
}