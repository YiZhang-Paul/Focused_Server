using Core.Models.Generic;
using System;

namespace Core.Models.TimeSession
{
    public class BreakSession : TimeRange
    {
        public string FocusSessionId { get; set; }
        public double TargetDuration { get; set; }
        public bool IsLongBreak { get; set; }
        public DateTime TargetEndTime => StartTime.AddHours(TargetDuration);
    }
}
