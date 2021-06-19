using System.Collections.Generic;

namespace Core.Models.WorkItem
{
    public class TimeSeries
    {
        public double ManualTracking { get; set; }
        public List<TimeEvent> AutoTracking { get; set; } = new List<TimeEvent>();
    }
}
