using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Core.Models.TimeSession
{
    public class BreakSession : UserOwnedRecord
    {
        public DateTime StartTime { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? EndTime { get; set; }
        public string FocusSessionId { get; set; }
        public double TargetDuration { get; set; }
        public bool IsLongBreak { get; set; }
    }
}
