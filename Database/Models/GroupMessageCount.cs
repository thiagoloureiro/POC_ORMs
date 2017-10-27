using System;

namespace PerformanceTest.Database.Models
{
    internal class GroupMessageCount
    {
        public Guid GroupId { get; set; }
        public int Count { get; set; }
    }
}
