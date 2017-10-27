using System;
using Xunit;
using PerformanceTest.Database.Models;
using PerformanceTest.Database.Impl;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceTest.Database
{
    public class DatabaseFlowTests
    {
        private Type[] databaseImplementations;
        private const int TestPasses = 3;

        public DatabaseFlowTests()
        {
            databaseImplementations = new Type[]
            {
                typeof(EFUoWDatabase),
                typeof(EFPreInstanceDatabase),
                typeof(PetaPocoDatabase),
                typeof(DapperDatabase),
                typeof(DapperSPDatabase),
                typeof(TSqlDatabase),
                typeof(TSqlSPDatabase)
            };
        }

        [Fact]
        public void CRUD_Batch()
        {
            var perf = new PerformanceMonitor();

            perf.Passes(TestPasses).ForEach<IDatabase>(databaseImplementations, db =>
            {
                var group = new Group();

                using (perf.Start("Create a new group"))
                {
                    db.BeginTransaction();
                    db.InsertGroup(group);
                    db.Commit();
                }

                using (perf.Start("Insert 200 messages"))
                {
                    db.BeginTransaction();
                    for (int i = 0; i < 200; i++)
                    {
                        db.InsertMessage(new Message(group.Id));
                    }
                    db.Commit();
                }

                Message[] messages;
                using (perf.Start("Get Group Messages (After insert)"))
                {
                    db.BeginTransaction();
                    messages = db.GetGroupMessages(group.Id);
                }

                using (perf.Start("Update 100 Messages"))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        messages[i].Text += " *Updated";
                        db.UpdateMessage(messages[i]);
                    }
                    db.Commit();
                }

                using (perf.Start("Delete 50 Messages"))
                {
                    var messagesToDelete = messages.Skip(75).Take(50).ToArray();
                    db.DeleteMessages(messagesToDelete);
                    db.Commit();
                }

                Dictionary<Guid, int> groupMessageCount;
                using (perf.Start("Group total messages Dictionary"))
                {
                    groupMessageCount = db.GroupMessageCounts();
                }

                var groupsToLoad = groupMessageCount.OrderBy(c => c.Value).Take(20).Select(c=> c.Key).ToArray();
                using (perf.Start("Eager Load Group with Messages"))
                {
                    db.FindGroupsDeep(groupsToLoad);
                }
            }, db=> db.WarmUp());

            //perf.Write($@"..\..\crud-batch-results-{DateTime.Now.ToString("yy-mm-dd_hh-mm-ss")}");
            perf.Write($@"..\..\crud-batch-results");
        }

        [Fact]
        public void Query_LargeGroup()
        {
            var perf = new PerformanceMonitor();

            perf.Passes(TestPasses).ForEach<IDatabase>(databaseImplementations, db =>
            {
                var groupId = new Guid("05609CBC-4925-4578-B653-C456B177B835");
                Group group;

                using (perf.Start("Load Group By Id"))
                {
                    group = db.FindGroup(groupId);
                    db.Commit();
                }

                Message[] messages;
                using (perf.Start("Get ALL Group Messages (150,000)"))
                {
                    messages = db.GetGroupMessages(group.Id);
                }

                using (perf.Start("Get Last Message"))
                {
                    db.GetLastGroupMessage(group.Id);
                }
            }, db => db.WarmUp());

            //perf.Write($@"..\..\crud-batch-results-{DateTime.Now.ToString("yy-mm-dd_hh-mm-ss")}");
            perf.Write($@"..\..\query-largegroup-results");
        }

        [Fact]
        public void Query_Limit_DateRange()
        {
            var perf = new PerformanceMonitor();

            perf.Passes(TestPasses).ForEach<IDatabase>(databaseImplementations, db =>
            {
                var groupId = new Guid("05609CBC-4925-4578-B653-C456B177B835");
                Group group;

                using (perf.Start("Load Group By Id"))
                {
                    group = db.FindGroup(groupId);
                }

                Message[] messages;
                using (perf.Start("Get Filtered By Daterange with Limit Desc (top 25 - 1 week)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 25, 7, "desc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Asc (top 25 - 1 week)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 25, 7, "asc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Desc (top 25 - 1 month)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 25, 30, "desc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Asc (top 25 - 1 month)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 25, 30, "asc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Desc (top 25 - 1 year)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 25, 365, "desc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Asc (top 25 - 1 year)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 25, 365, "asc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Desc (top 100 - 1 week)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 100, 7, "desc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Asc (top 100 - 1 week)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 100, 7, "asc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Desc (top 100 - 1 month)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 100, 30, "desc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Asc (top 100 - 1 month)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 100, 30, "asc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Desc (top 100 - 1 year)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 100, 365, "desc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Asc (top 100 - 1 year)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 100, 365, "asc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Desc (top 1000 - 1 week)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 1000, 7, "desc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Asc (top 1000 - 1 week)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 1000, 7, "asc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Desc (top 1000 - 1 month)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 1000, 30, "desc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Asc (top 1000 - 1 month)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 1000, 30, "asc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Desc (top 1000 - 1 year)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 1000, 365, "desc");
                }

                using (perf.Start("Get Filtered By Daterange with Limit Asc (top 1000 - 1 year)"))
                {
                    messages = db.GetMessagesLimitByDate(group.Id, 1000, 365, "asc");
                }
            }, db => db.WarmUp());

            //perf.Write($@"..\..\crud-batch-results-{DateTime.Now.ToString("yy-mm-dd_hh-mm-ss")}");
            perf.Write($@"..\..\query-limit-datarange-results");
        }
    }
}