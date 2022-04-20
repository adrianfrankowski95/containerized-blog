using System.Data.Common;
using Dapper;

namespace Blog.Services.Blogging.API.Extensions;

public static class DbDataReaderExtensions
{
    public static async IAsyncEnumerable<T> StreamAsync<T>(this DbDataReader reader)
    {
        var rowParser = reader.GetRowParser<T>();
        while (await reader.ReadAsync().ConfigureAwait(false))
            yield return rowParser(reader);
    }
}
