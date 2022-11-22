using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Blog.Services.Identity.Infrastructure.Converters;

public class NonNegativeIntConverter : ValueConverter<NonNegativeInt, int>
{
    public NonNegativeIntConverter()
        : base(v => v.Value, v => new NonNegativeInt(v))
    {

    }
}
