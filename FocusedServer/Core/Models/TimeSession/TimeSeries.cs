using Core.Enums;
using Core.Models.Generic;

namespace Core.Models.TimeSession
{
    public class TimeSeries : TimeRange
    {
        public TimeSeriesType Type { get; set; }
        public string DataSourceId { get; set; }
    }
}
