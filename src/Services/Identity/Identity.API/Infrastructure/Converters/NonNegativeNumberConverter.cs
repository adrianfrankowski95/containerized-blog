using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Blog.Services.Identity.API.Infrastructure.Converters;

public class NonNegativeNumberConverter : ValueConverter<NonNegativeNumber, int>
{
    public NonNegativeNumberConverter() :
        base(x => x.Value, x => new NonNegativeNumber(x))
    {
    }
}
