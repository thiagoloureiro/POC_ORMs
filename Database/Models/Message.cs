using Dapper.Contrib.Extensions;
using PetaPoco;
using System;

namespace PerformanceTest.Database.Models
{
    [TableName("Messages")]
    public class Message
    {
        public Message()
        {

        }

        public Message(Guid groupId)
        {
            GroupId = groupId;
        }

        [ExplicitKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid GroupId { get; set; }

        public Guid? ParentId { get; set; }

        public Guid CreatedById { get; set; }

        public Guid TypeId { get; set; } = Guid.NewGuid();

        public string Text { get; set; } = $"PerformanceTest Message: {Guid.NewGuid()}";

        public string AttachmentUrl { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; } = DateTime.UtcNow;


        [Write(false), Ignore]
        public virtual Message Parent { get; set; }

        [Write(false), Ignore]
        public bool IsRoot => ParentId == null;
    }
}
