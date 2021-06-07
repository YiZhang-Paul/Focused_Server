using Core.Enums;

namespace Core.Dtos
{
    public class WorkItemProgressionDto
    {
        public string Id { get; set; }
        public WorkItemType Type { get; set; }
        public double HoursSpent { get; set; }
    }
}
