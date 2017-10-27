using Dapper.Contrib.Extensions;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace PerformanceTest.Database.Models
{
    [TableName("Groups")]
    public class Group
    {
        [ExplicitKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = $"PerformanceTest: {Guid.NewGuid()}";

        public bool IsPublic { get; set; }

        public string AvatarUrl { get; set; }

        public Guid TypeId { get; set; } = Guid.NewGuid();

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? ArchivedOn { get; set; }

        [Write(false), Ignore]
        public virtual ICollection<Message> Messages { get; set; }
    }
}
