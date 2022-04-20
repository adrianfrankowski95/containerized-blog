using System.Data;
using Dapper;
using NodaTime;

namespace Blog.Services.Blogging.API.Infrastructure.TypeHandlers;

public class InstantTypeHandler : SqlMapper.TypeHandler<Instant>
{
    public InstantTypeHandler()
    {
    }

    public override Instant Parse(object value)
    {
        if (value is DateTime dateTime)
            return Instant.FromDateTimeUtc(dateTime);

        if (value is Instant instant)
            return instant;

        throw new DataException($"Unable to convert {value} to {nameof(Instant)}");
    }

    public override void SetValue(IDbDataParameter parameter, Instant value)
    {
        parameter.Value = value;
    }
}

