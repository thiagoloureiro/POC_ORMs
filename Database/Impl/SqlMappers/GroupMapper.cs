using PerformanceTest.Database.Models;
using System.Data.SqlClient;

namespace PerformanceTest.Database.Impl.SqlMappers
{
    internal class GroupMapper : BaseMapper<Group>
    {
        internal override Group Map(SqlDataReader reader)
        {
            return new Group
            {
                Id = reader.GetGuid(0),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                IsPublic = reader.GetBoolean(2),
                AvatarUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                CreatedOn = reader.GetDateTime(5),
                UpdatedOn = reader.GetNullableDateTime(6),
                ArchivedOn = reader.GetNullableDateTime(7)
            };
        }
    }
}