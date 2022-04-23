using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Blog.Services.Identity.API.Infrastructure.Converters;

public class NonEmptyStringConverter : ValueConverter<NonEmptyString, string>
{
    public NonEmptyStringConverter() :
        base(x => x.Value, x => new NonEmptyString(x))
    {
    }
}
