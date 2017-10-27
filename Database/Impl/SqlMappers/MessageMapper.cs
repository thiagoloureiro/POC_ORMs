using PerformanceTest.Database.Models;
using System;
using System.Data.SqlClient;

namespace PerformanceTest.Database.Impl.SqlMappers
{
    internal class MessageMapper : BaseMapper<Message>
    {
        internal override Message Map(SqlDataReader reader)
        {
            return new Message
            {
                Id = reader.GetGuid(0),
                GroupId = reader.GetGuid(1),
                CreatedById = reader.GetGuid(3),
                Text = reader.IsDBNull(5) ? null : reader.GetString(5),
                AttachmentUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                CreatedOn = reader.GetDateTime(7),
                UpdatedOn = reader.GetNullableDateTime(8),
                ParentId = reader.IsDBNull(2) ? (Guid?)null : (Guid?)reader.GetGuid(2)
            };
        }
    }
}