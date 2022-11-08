namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public enum SecurityStampValidationResult
{
    Valid = 0,
    Invalid = 1,
    NoValidationNeeded = 2,
}

