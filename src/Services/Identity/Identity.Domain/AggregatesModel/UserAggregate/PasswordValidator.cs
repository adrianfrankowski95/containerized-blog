namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class PasswordValidator : IPasswordValidator
{
    private readonly List<IRequirement<Password>> _requirements;
    public IReadOnlyList<IRequirement<Password>> Requirements => _requirements;

    public PasswordValidator(List<IRequirement<Password>> requirements)
    {
        if(requirements is null || requirements.Count == 0)
            throw new ArgumentNullException(nameof(requirements));

        _requirements = requirements;
    }

    public static PasswordValidator DefaultValidator()
        => new(PasswordRequirementsFactory.DefaultSet());

    public ValidationResult<Password> Validate(Password password)
    {
        var violatedRequirements = _requirements.Where(r => !r.IsSatisfiedBy(password));

        return violatedRequirements.Any()
            ? ValidationResult<Password>.Fail(violatedRequirements.Select(m => m.Message))
            : ValidationResult<Password>.Success();
    }
}