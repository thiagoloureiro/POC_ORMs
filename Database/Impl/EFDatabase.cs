using System;
using PerformanceTest.Database.Models;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace PerformanceTest.Database.Impl
{
    public abstract class EFDatabase : IDatabase
    {
        public void WarmUp()
        {
            Execute(db => db.Groups.FirstOrDefault());
            Execute(db => db.Messages.FirstOrDefault());
        }

        public Message[] GetAllMessages()
        {
            return Execute(db => db.Messages.OrderByDescending(x => x.CreatedOn).ToArray());
        }

        public Message[] GetGroupMessages(Guid groupId)
        {
            return Execute(db =>
                db.Messages
                    .Where(m => m.GroupId == groupId)
                    .OrderByDescending(m => m.CreatedOn)
                    .ToArray()
            );
        }

        public Message GetLastGroupMessage(Guid groupId)
        {
            return Execute(db => db.Messages
                .Where(m => m.GroupId == groupId)
                .OrderByDescending(m => m.CreatedOn)
                .FirstOrDefault());
        }

        public Dictionary<Guid, int> GroupMessageCounts()
        {
            return Execute(db => db.Groups.Select(c => new
            {
                id = c.Id,
                count = c.Messages.Count
            }).ToDictionary(c => c.id, c => c.count));
        }

        public Group[] FindGroupsDeep(Guid[] ids)
        {
            return Execute(db => db.Groups
                .Where(g => ids.Contains(g.Id))
                .Include(g => g.Messages)
                .ToArray()
            );
        }

        public Group FindGroup(Guid id)
        {
            return Execute(db => db.Groups.Find(id));
        }

        public Group InsertGroup(Group group)
        {
            return Execute(db => db.Groups.Add(group));
        }

        public Message InsertMessage(Message message)
        {
            return Execute(db => db.Messages.Add(message));
        }

        public Message UpdateMessage(Message message)
        {
            // EF Uses change tracking so nothing required
            return message;
        }

        public void DeleteMessages(Message[] messages)
        {
            var ids = messages.Select(m => m.Id);
            Execute(db =>
            {
                var items = db.Messages.Where(m => ids.Contains(m.Id));
                db.Messages.RemoveRange(items);
            });
        }

        public abstract void BeginTransaction();

        public abstract void Commit();

        public Message[] GetMessagesLimitByDate(Guid groupId, int limit, int daysRange, string order)
        {
            return Execute(db =>
            {
                var dtStart = Convert.ToDateTime("2015-01-01 00:00:00");
                var dtEnd = dtStart.AddDays(daysRange);

                if (order == "desc")
                    return db.Messages.Where(x => x.GroupId == groupId && (x.CreatedOn >= dtStart && x.CreatedOn <= dtEnd)).Take(limit).OrderByDescending(m => m.CreatedOn).ToArray();
                else
                    return db.Messages.Where(x => x.GroupId == groupId && (x.CreatedOn >= dtStart && x.CreatedOn <= dtEnd)).Take(limit).OrderBy(m => m.CreatedOn).ToArray();
            });
        }

        protected abstract TReturn Execute<TReturn>(Func<DataContext, TReturn> action);

        protected abstract void Execute(Action<DataContext> action);
    }

    public class DataContext : DbContext
    {
        public DataContext()
            : base("AmicoChat")
        {
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
    }
}