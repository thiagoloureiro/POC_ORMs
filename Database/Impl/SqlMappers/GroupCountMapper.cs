using PerformanceTest.Database.Models;
using System.Data.SqlClient;

namespace PerformanceTest.Database.Impl.SqlMappers
{
    internal class GroupCountMapper : BaseMapper<GroupMessageCount>
    {
        internal override GroupMessageCount Map(SqlDataReader reader)
        {
            return new GroupMessageCount
            {
                GroupId = reader.GetGuid(0),
                Count = reader.GetInt32(1),
            };
        }
    }
}
