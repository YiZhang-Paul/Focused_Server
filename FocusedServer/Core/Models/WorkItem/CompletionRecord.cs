using System;

namespace Core.Models.WorkItem
{
    public class CompletionRecord
    {
        public DateTime Time { get; set; }
        public bool IsPastDue { get; set; }
    }
}
