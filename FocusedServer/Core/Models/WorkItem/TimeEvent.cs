using Core.Enums;
using System;

namespace Core.Models.WorkItem
{
    public class TimeEvent
    {
        public DateTime Time { get; set; }
        public TimeEventType Type { get; set; }
    }
}
