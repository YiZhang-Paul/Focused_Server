using Core.Models.TimeSession;
using System.Collections.Generic;

namespace Core.Models.Aggregates
{
    public class WorkItemWithTimeSeries : WorkItem.WorkItem
    {
        public string WorkItemId { get; set; }
        public List<TimeSeries> TimeSeries { get; set; }
    }
}
