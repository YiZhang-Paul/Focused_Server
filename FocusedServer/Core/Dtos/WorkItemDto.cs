using Core.Enums;
using Core.Models.Generic;

namespace Core.Dtos
{
    public class WorkItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public WorkItemType Type { get; set; } = WorkItemType.Regular;
        public WorkItemPriority Priority { get; set; } = WorkItemPriority.NotUrgentNotImportant;
        public WorkItemStatus Status { get; set; } = WorkItemStatus.Idle;
        public ProgressionCounter<double> ItemProgress { get; set; } = new ProgressionCounter<double>();
        public ProgressionCounter<int> SubtaskProgress { get; set; } = new ProgressionCounter<int>();
        public ProgressionCounter<int> ChecklistProgress { get; set; } = new ProgressionCounter<int>();
    }
}
