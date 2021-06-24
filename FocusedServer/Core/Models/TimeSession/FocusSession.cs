using System;
using System.Collections.Generic;

namespace Core.Models.TimeSession
{
    public class FocusSession : UserOwnedRecord
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double TargetDuration { get; set; }
        public double OverlearningHours { get; set; }
        public List<string> WorkItemIds { get; set; } = new List<string>();
    }
}
