namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface IPasswordValidator : IValidator<Password, ValidationResult<Password>>
{
   
}