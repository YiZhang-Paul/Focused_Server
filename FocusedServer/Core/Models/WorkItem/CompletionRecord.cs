using System;

namespace Core.Models.WorkItem
{
    public class CompletionRecord
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public bool IsPastDue { get; set; }
    }
}
