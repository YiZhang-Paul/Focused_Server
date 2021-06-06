using Core.Enums;
using Core.Models.Generic;

namespace Core.Dtos
{
    public class WorkItemDto
    {
        public string Name { get; set; }
        public WorkItemType Type { get; set; } = WorkItemType.Regular;
        public WorkItemPriority Priority { get; set; } = WorkItemPriority.NotUrgentNotImportant;
        public int Estimation { get; set; }
        public ProgressionCounter<int> SubtaskProgress { get; set; } = new ProgressionCounter<int>();
        public ProgressionCounter<int> ChecklistProgress { get; set; } = new ProgressionCounter<int>();
        public bool IsCompleted { get; set; }
    }
}
