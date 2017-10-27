using Dapper;
using Dapper.Contrib.Extensions;
using PerformanceTest.Database.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PerformanceTest.Database.Impl
{
    public class DapperSPDatabase : IDatabase
    {
        private readonly string _connectionString;

        public DapperSPDatabase()
        {
            _connectionString =
                ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString;
        }

        public void WarmUp()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                var sql = @"select TOP 1 * from messages";
                con.Query<Message>(sql).ToList();

                sql = @"select TOP 1 * from groups";
                con.Query<Group>(sql).ToList();
            }
        }
        
        public Message[] GetAllMessages()
        {
            Message[] ret;
            using (var con = new SqlConnection(_connectionString))
            {
                ret = con.Query<Message>("MessagesSelectAll", commandType: CommandType.StoredProcedure).ToArray();
            }
            return ret;
        }

        public Message[] GetGroupMessages(Guid groupId)
        {
            List<Message> ret;
            using (var con = new SqlConnection(_connectionString))
            {
                ret = con.Query<Message>("MessagesSelectByGroup", new { GroupId = groupId }, commandType: CommandType.StoredProcedure).ToList();
            }
            return ret.ToArray();
        }

        public Message GetLastGroupMessage(Guid groupId)
        {
            Message ret;
            using (var con = new SqlConnection(_connectionString))
            {
                ret = con.Query<Message>("MessagesSelectLastByGroup", new { GroupId = groupId }, commandType: CommandType.StoredProcedure).Single();
            }
            return ret;
        }

        public Group FindGroup(Guid id)
        {
            Group ret;
            using (var db = new SqlConnection(_connectionString))
            {
                ret = db.QueryFirstOrDefault<Group>("GroupsSelect", new { Id = id }, commandType: CommandType.StoredProcedure);
            }
            return ret;
        }

        public Dictionary<Guid, int> GroupMessageCounts()
        {
            Dictionary<Guid, int> ret;
            using (var con = new SqlConnection(_connectionString))
            {
                var results = con.Query<GroupMessageCount>("GroupsMessageCount", commandType: CommandType.StoredProcedure);
                ret = results.ToDictionary(c => c.GroupId, c=> c.Count);
            }
            return ret;
        }

        public Group[] FindGroupsDeep(Guid[] ids)
        {
            Group[] ret;
            using (var con = new SqlConnection(_connectionString))
            {
                var sql = @"
SELECT * FROM Groups WHERE Id IN @ids;
SELECT * FROM Messages WHERE GroupId IN @ids;
";
                var results = con.QueryMultiple(sql, new { ids = ids });

                var groups = results.Read<Group>();
                var messages = results.Read<Message>();
                foreach (var group in groups)
                {
                    group.Messages = messages.Where(m => m.GroupId == group.Id).ToList();
                }
                ret = groups.ToArray();
            }
            return ret;
        }

        public Group InsertGroup(Group group)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Execute("GroupsInsert", new
                {
                    Id = group.Id,
                    Name = group.Name,
                    IsPublic = group.IsPublic,
                    AvatarUrl = group.AvatarUrl,
                    TypeId = group.TypeId,
                    CreatedOn = group.CreatedOn,
                    UpdatedOn = group.UpdatedOn,
                    ArchivedOn = group.ArchivedOn
                }, commandType: CommandType.StoredProcedure);
            }
            return group;
        }

        public Message InsertMessage(Message message)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Execute("MessagesInsert", new
                {
                    Id = message.Id,
                    GroupId = message.GroupId,
                    CreatedById = message.CreatedById,
                    TypeId = message.TypeId,
                    Text = message.Text,
                    AttachmentUrl = message.AttachmentUrl,
                    CreatedOn = message.CreatedOn,
                    UpdatedOn = message.UpdatedOn,
                    ParentId = message.ParentId
                }, commandType: CommandType.StoredProcedure);
            }
            return message;
        }

        public Message UpdateMessage(Message message)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Execute("MessagesUpdate", new
                {
                    Id = message.Id,
                    GroupId = message.GroupId,
                    CreatedById = message.CreatedById,
                    TypeId = message.TypeId,
                    Text = message.Text,
                    AttachmentUrl = message.AttachmentUrl,
                    CreatedOn = message.CreatedOn,
                    UpdatedOn = message.UpdatedOn,
                    ParentId = message.ParentId
                }, commandType: CommandType.StoredProcedure);
            }
            return message;
        }

        public void DeleteMessages(Message[] messages)
        {
            var messageIds = messages.Select(m => $"'{m.Id}'");
            using (var con = new SqlConnection(_connectionString))
            {
                con.Execute($@"
    DELETE FROM [Messages]
    WHERE Id IN ({string.Join(",", messageIds)})"
                );

            }
        }

        public void BeginTransaction()
        {

        }

        public void Commit()
        {
        }

        public Message[] GetMessagesLimitByDate(Guid groupId, int limit, int daysRange, string order)
        {
            List<Message> ret;

            var dtStart = Convert.ToDateTime("2015-01-01 00:00:00");
            var dtEnd = dtStart.AddDays(daysRange);

            using (var con = new SqlConnection(_connectionString))
            {
                var sql = $"select top {limit} * from messages where GroupId = @GroupId and Createdon between @DtStart and @DtEnd order by CreatedOn {order}";
                ret = con.Query<Message>(sql, new { GroupId = groupId, DtStart = dtStart, DtEnd = dtEnd }).ToList();
            }
            return ret.ToArray();
        }
    }
}