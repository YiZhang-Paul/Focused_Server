using Core.Enums;
using Core.Models.Generic;
using System.Collections.Generic;

namespace Core.Models.WorkItem
{
    public class WorkItem : DatabaseEntry
    {
        public string Parent { get; set; } = null;
        public string Name { get; set; }
        public string Description { get; set; }
        public WorkItemType Type { get; set; } = WorkItemType.Regular;
        public WorkItemPriority Priority { get; set; } = WorkItemPriority.NotUrgentNotImportant;
        public int Estimation { get; set; }
        public bool[] Recur { get; set; } = new bool[7];
        public List<ChecklistEntry> Checklists { get; set; } = new List<ChecklistEntry>();
        public TimeInfo TimeInfo { get; set; } = new TimeInfo();
        public bool IsCompleted { get; set; }
    }
}
