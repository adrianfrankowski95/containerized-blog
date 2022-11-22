using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace Blog.Services.Identity.Infrastructure.Converters;

public class NonPastInstantConverter : ValueConverter<NonPastInstant, Instant>
{
    public NonPastInstantConverter()
        : base(v => v.Value, v => new NonPastInstant(v, default(Instant)))
    {

    }
}
