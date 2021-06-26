using Core.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Core.Models.TimeSession
{
    public class TimeSeries : UserOwnedRecord
    {
        public DateTime StartTime { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? EndTime { get; set; }
        public TimeSeriesType Type { get; set; }
        public string DataSourceId { get; set; }
    }
}
