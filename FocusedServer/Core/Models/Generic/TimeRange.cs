using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Core.Models.Generic
{
    public class TimeRange : UserOwnedRecord
    {
        public DateTime StartTime { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? EndTime { get; set; }
    }
}
