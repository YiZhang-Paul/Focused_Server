using System;

namespace Core.Models
{
    public class BreakSession : DatabaseEntry
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string FocusSessionId { get; set; }
        public double TargetDuration { get; set; }
        public bool IsLongBreak { get; set; }
    }
}
