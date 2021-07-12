using Core.Models.Generic;
using System;

namespace Core.Models.TimeSession
{
    public class FocusSession : TimeRange
    {
        public double TargetDuration { get; set; }
        public DateTime TargetEndTime => StartTime.AddHours(TargetDuration);
    }
}
