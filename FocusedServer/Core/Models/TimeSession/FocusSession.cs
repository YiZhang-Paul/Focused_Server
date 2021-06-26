using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Core.Models.TimeSession
{
    public class FocusSession : UserOwnedRecord
    {
        public DateTime StartTime { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? EndTime { get; set; }
        public double TargetDuration { get; set; }
    }
}
