using Dapper;
using Dapper.Contrib.Extensions;
using PerformanceTest.Database.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace PerformanceTest.Database.Impl
{
    public class DapperDatabase : IDatabase
    {
        private readonly string _connectionString;

        public DapperDatabase()
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
            List<Message> ret;
            using (var con = new SqlConnection(_connectionString))
            {
                var sql = @"select * from messages";
                ret = con.Query<Message>(sql).ToList();
            }
            return ret.ToArray();
        }

        public Message[] GetGroupMessages(Guid groupId)
        {
            List<Message> ret;
            using (var con = new SqlConnection(_connectionString))
            {
                var sql = @"select * from messages where GroupId = @GroupId order by CreatedOn desc";
                ret = con.Query<Message>(sql, new { GroupId = groupId }).ToList();
            }
            return ret.ToArray();
        }

        public Message GetLastGroupMessage(Guid groupId)
        {
            Message ret;
            using (var con = new SqlConnection(_connectionString))
            {
                var sql = @"select top 1 * from messages where GroupId = @GroupId order by CreatedOn Desc";
                ret = con.Query<Message>(sql, new { GroupId = groupId }).Single();
            }
            return ret;
        }

        public Group FindGroup(Guid id)
        {
            Group ret;
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = @"select top 1 * from Groups where Id = @Id";
                ret = db.QueryFirstOrDefault<Group>(sql, new { Id = id });
            }
            return ret;
        }

        public Dictionary<Guid, int> GroupMessageCounts()
        {
            Dictionary<Guid, int> ret;
            using (var con = new SqlConnection(_connectionString))
            {
                var sql = @"
SELECT 
    GroupId, 
    COUNT(*) AS Count
FROM Messages 
GROUP BY GroupId";

                var results = con.Query<GroupMessageCount>(sql);
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
                con.Insert(group);
            }
            return group;
        }

        public Message InsertMessage(Message message)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Insert(message);
            }
            return message;
        }

        public Message UpdateMessage(Message message)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Update(message);
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