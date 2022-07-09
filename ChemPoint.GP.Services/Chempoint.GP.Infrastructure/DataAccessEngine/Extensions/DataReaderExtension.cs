using System;
using System.Collections.Generic;
using System.Data;

namespace Chempoint.GP.Infrastructure.DataAccessEngine.Extensions
{
    public static class DataReaderExtension
    {
        public static IEnumerable<T> Select<T>(this IDataReader reader, Func<IDataReader, T> projection)
        {
            while (reader.Read())
            {
                yield return projection(reader);
            }
        }

        public static T SelectRow<T>(this DataRow row, Func<DataRow, T> projection)
        {
            return projection(row);
        }
    }
}
