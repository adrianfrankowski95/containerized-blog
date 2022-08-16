namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class PasswordValidator : IValidator<Password>
{
    public IEnumerable<IRequirement<Password>>? Requirements { get; }

    public PasswordValidator(IEnumerable<IRequirement<Password>> requirements)
    {
        Requirements = requirements;
    }

    public ValidationResult<Password> Validate(Password password)
    {
        if(Requirements is null || !Requirements.Any())
            return ValidationResult<Password>.Success;

        var violatedRequirements = Requirements.Where(r => !r.IsSatisfiedBy(password));

        return violatedRequirements.Any()
            ? ValidationResult<Password>.Fail(violatedRequirements.Select(m => m.Message))
            : ValidationResult<Password>.Success;
    }
}