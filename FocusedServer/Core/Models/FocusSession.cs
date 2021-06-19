using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class FocusSession : DatabaseEntry
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double OverlearningHours { get; set; }
        public List<string> WorkItemIds { get; set; } = new List<string>();
    }
}
