using Core.Enums;
using System;

namespace Core.Models.TimeSession
{
    public class TimeSeries : UserOwnedRecord
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSeriesType Type { get; set; }
        public string DataSourceId { get; set; }
    }
}
