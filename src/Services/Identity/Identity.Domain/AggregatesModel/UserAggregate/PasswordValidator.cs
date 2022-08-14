namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class PasswordValidator : IPasswordValidator
{
    public IEnumerable<IRequirement<Password>> Requirements { get; }

    public PasswordValidator(IEnumerable<IRequirement<Password>> requirements)
    {
        if (requirements is null || !requirements.Any())
            throw new ArgumentNullException(nameof(requirements));

        Requirements = requirements;
    }

    public ValidationResult<Password> Validate(Password password)
    {
        var violatedRequirements = Requirements.Where(r => !r.IsSatisfiedBy(password));

        return violatedRequirements.Any()
            ? ValidationResult<Password>.Fail(violatedRequirements.Select(m => m.Message))
            : ValidationResult<Password>.Success();
    }
}