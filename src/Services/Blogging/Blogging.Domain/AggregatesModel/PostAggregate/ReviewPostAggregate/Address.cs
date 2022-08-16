using System.Text.RegularExpressions;
using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;

public sealed class Address : ValueObject<Address>, IValidatable
{
    public string Country { get; }
    public string ZipCode { get; }
    public string City { get; }
    public string Street { get; }

    //ef core
    private Address() { }

    public Address(string country, string zipCode, string city, string street)
    {
        Country = country;
        ZipCode = zipCode;
        City = city;
        Street = street;
    }
    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Country;
        yield return ZipCode;
        yield return City;
        yield return Street;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Country))
            throw new BloggingDomainException($"{nameof(Country)} cannot be null or empty");

        if (string.IsNullOrWhiteSpace(ZipCode))
            throw new BloggingDomainException($"{nameof(ZipCode)} cannot be null or empty");

        if (string.IsNullOrWhiteSpace(City))
            throw new BloggingDomainException($"{nameof(City)} cannot be null or empty");

        if (string.IsNullOrWhiteSpace(Street))
            throw new BloggingDomainException($"{nameof(Street)} cannot be null or empty");

        if (!new Regex(@"^[a-z0-9][a-z0-9\- ]{0,10}[a-z0-9]$").Match(ZipCode).Success)
            throw new BloggingDomainException($"{nameof(ZipCode)} has a wrong format");
    }
}
