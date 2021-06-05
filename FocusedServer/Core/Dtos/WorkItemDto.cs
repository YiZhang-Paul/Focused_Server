using Core.Enums;

namespace Core.Dtos
{
    public class WorkItemDto
    {
        public string Name { get; set; }
        public WorkItemType Type { get; set; } = WorkItemType.Regular;
        public WorkItemPriority Priority { get; set; } = WorkItemPriority.NotUrgentNotImportant;
        public int Estimation { get; set; }
    }
}
