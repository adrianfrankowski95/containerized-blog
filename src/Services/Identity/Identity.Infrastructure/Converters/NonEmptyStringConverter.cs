using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Blog.Services.Identity.Infrastructure.Converters;

public class NonEmptyStringConverter : ValueConverter<NonEmptyString, string>
{
    public NonEmptyStringConverter()
        : base(v => v.Value, v => new NonEmptyString(v))
    {

    }
}