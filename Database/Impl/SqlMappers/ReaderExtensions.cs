using System;
using System.Data.SqlClient;

namespace PerformanceTest.Database.Impl.SqlMappers
{
    public static class ReaderExtensions
    {

        public static DateTime? GetNullableDateTime(this SqlDataReader reader, int col)
        {
            return reader.IsDBNull(col) ?
                        (DateTime?)null :
                        (DateTime?)reader.GetDateTime(col);
        }

        public static DateTime? GetNullableDateTime(this SqlDataReader reader, string name)
        {
            var col = reader.GetOrdinal(name);
            return reader.IsDBNull(col) ?
                        (DateTime?)null :
                        (DateTime?)reader.GetDateTime(col);
        }
    }
}
