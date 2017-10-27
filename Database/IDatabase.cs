using PerformanceTest.Database.Models;
using System;
using System.Collections.Generic;

namespace PerformanceTest.Database
{
    public interface IDatabase
    {
        void WarmUp();

        Message[] GetAllMessages();
        
        Message[] GetGroupMessages(Guid groupId);

        Message GetLastGroupMessage(Guid groupId);
        
        Group[] FindGroupsDeep(Guid[] groupIds);

        Dictionary<Guid, int> GroupMessageCounts();

        Group FindGroup(Guid group);

        Group InsertGroup(Group group);

        Message InsertMessage(Message message);

        Message UpdateMessage(Message message);

        void DeleteMessages(Message[] messages);

        void BeginTransaction();
        void Commit();

        Message[] GetMessagesLimitByDate(Guid groupId, int limit, int daysRange, string order);
        
    }
}