using PerformanceTest.Database.Impl.SqlMappers;
using PerformanceTest.Database.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace PerformanceTest.Database.Impl
{
    public class TSqlDatabase : IDatabase
    {
        private readonly string _connectionString;

        public TSqlDatabase()
        {
            _connectionString =
                ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString;
        }

        public void WarmUp()
        {
            var mapper = new MessageMapper();
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = @"select TOP 1 * from messages",
                    Connection = con
                };

                mapper.ReadMultiple(command);
            }
        }

        public Message[] GetAllMessages()
        {
            var mapper = new MessageMapper();
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = @"select * from messages order by CreatedOn Desc",
                    Connection = con
                };

                return mapper.ReadMultiple(command);
            }
        }

        public Message[] GetGroupMessages(Guid groupId)
        {
            var mapper = new MessageMapper();
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = @"select * from messages where GroupId = @GroupId order by CreatedOn Desc",
                    Connection = con
                };
                command.Parameters.AddWithValue("@GroupId", groupId);

                return mapper.ReadMultiple(command);
            }
        }

        public Message GetLastGroupMessage(Guid groupId)
        {
            var mapper = new MessageMapper();
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = @"select top 1 * from messages where GroupId = @GroupId order by CreatedOn Desc",
                    Connection = con
                };
                command.Parameters.AddWithValue("@GroupId", groupId);

                return mapper.ReadSingle(command);
            }
        }

        public Group FindGroup(Guid id)
        {
            var mapper = new GroupMapper();
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = "SELECT * FROM Groups WHERE Id = @id",
                    Connection = con
                };
                command.Parameters.AddWithValue("@id", id);

                return mapper.ReadSingle(command);
            }
        }

        public Dictionary<Guid, int> GroupMessageCounts()
        {
            var mapper = new GroupCountMapper();
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = @"
SELECT
    GroupId,
    COUNT(*) AS Count
FROM Messages
GROUP BY GroupId",
                    Connection = con
                };

                return mapper.ReadMultiple(command).ToDictionary(c => c.GroupId, c => c.Count);
            }
        }

        public Group[] FindGroupsDeep(Guid[] ids)
        {
            // TODO
            Group[] ret;

            var groupMapper = new GroupMapper();
            var messageMapper = new MessageMapper();

            var groupIds = string.Join(", ", ids.Select(i => $"'{i}'"));
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var groupCommand = new SqlCommand
                {
                    CommandText = $"SELECT * FROM Groups WHERE Id IN ({groupIds});",
                    Connection = con
                };
                ret = groupMapper.ReadMultiple(groupCommand);

                var messageCommand = new SqlCommand
                {
                    CommandText = $"SELECT * FROM Messages WHERE GroupId IN ({groupIds});",
                    Connection = con
                };
                var messages = messageMapper.ReadMultiple(messageCommand);

                foreach (var group in ret)
                {
                    group.Messages = messages.Where(m => m.GroupId == group.Id).ToList();
                }
            }
            return ret;
        }

        public Group InsertGroup(Group group)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = @"
INSERT INTO [dbo].[Groups]
           ([Id], [Name], [IsPublic], [AvatarUrl], [TypeId], [CreatedOn], [UpdatedOn], [ArchivedOn])
     VALUES
           (@id, @Name, @IsPublic, @AvatarUrl, @TypeId, @CreatedOn, @UpdatedOn, @ArchivedOn)",
                    Connection = con
                };
                command.Parameters.AddWithValue("@id", group.Id);
                command.Parameters.AddWithValue("@Name", group.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IsPublic", group.IsPublic);
                command.Parameters.AddWithValue("@AvatarUrl", group.AvatarUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TypeId", group.TypeId);
                command.Parameters.AddWithValue("@CreatedOn", group.CreatedOn);
                command.Parameters.AddWithValue("@UpdatedOn", group.UpdatedOn ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ArchivedOn", group.ArchivedOn ?? (object)DBNull.Value);

                command.ExecuteScalar();
            }
            return group;
        }

        public Message InsertMessage(Message message)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = @"
    INSERT INTO [dbo].[Messages]
           ([Id], [GroupId], [CreatedById], [TypeId], [Text], [AttachmentUrl], [CreatedOn], [UpdatedOn], [ParentId])
     VALUES
           (@Id, @GroupId, @CreatedById, @TypeId, @Text, @AttachmentUrl, @CreatedOn, @UpdatedOn, @ParentId)",
                    Connection = con
                };
                command.Parameters.AddWithValue("@Id", message.Id);
                command.Parameters.AddWithValue("@GroupId", message.GroupId);
                command.Parameters.AddWithValue("@CreatedById", message.CreatedById);
                command.Parameters.AddWithValue("@TypeId", message.TypeId);
                command.Parameters.AddWithValue("@Text", message.Text ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AttachmentUrl", message.AttachmentUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedOn", message.CreatedOn);
                command.Parameters.AddWithValue("@UpdatedOn", message.UpdatedOn ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ParentId", message.ParentId ?? (object)DBNull.Value);

                command.ExecuteScalar();
            }
            return message;
        }

        public Message UpdateMessage(Message message)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = @"
UPDATE [Messages]
   SET [GroupId] = @GroupId
      ,[CreatedById] = @CreatedById
      ,[Text] = @Text
      ,[AttachmentUrl] = @AttachmentUrl
      ,[CreatedOn] = @CreatedOn
      ,[UpdatedOn] = @UpdatedOn
      ,[ParentId] = @ParentId
 WHERE Id = @Id",
                    Connection = con
                };
                command.Parameters.AddWithValue("@Id", message.Id);
                command.Parameters.AddWithValue("@GroupId", message.GroupId);
                command.Parameters.AddWithValue("@CreatedById", message.CreatedById);
                command.Parameters.AddWithValue("@Text", message.Text ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AttachmentUrl", message.AttachmentUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedOn", message.CreatedOn);
                command.Parameters.AddWithValue("@UpdatedOn", message.UpdatedOn ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ParentId", message.ParentId ?? (object)DBNull.Value);

                command.ExecuteScalar();
            }
            return message;
        }

        public void DeleteMessages(Message[] messages)
        {
            var messageIds = messages.Select(m => $"'{m.Id}'");
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = $@"
    DELETE FROM [Messages]
    WHERE Id IN ({string.Join(",", messageIds)})",
                    Connection = con
                };

                command.ExecuteScalar();
            }
        }

        public Message[] GetMessagesLimitByDate(Guid groupId, int limit, int daysRange, string order)
        {
            var dtStart = Convert.ToDateTime("2015-01-01 00:00:00");
            var dtEnd = dtStart.AddDays(daysRange);

            var mapper = new MessageMapper();
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var command = new SqlCommand
                {
                    CommandText = $"select top {limit} * from messages where GroupId = @GroupId and Createdon between @DtStart and @DtEnd order by CreatedOn {order}",
                    Connection = con
                };
                command.Parameters.AddWithValue("@GroupId", groupId);
                command.Parameters.AddWithValue("@DtStart", dtStart);
                command.Parameters.AddWithValue("@DtEnd", dtEnd);

                return mapper.ReadMultiple(command);
            }
        }

        public void BeginTransaction()
        {
        }

        public void Commit()
        {
        }
    }
}