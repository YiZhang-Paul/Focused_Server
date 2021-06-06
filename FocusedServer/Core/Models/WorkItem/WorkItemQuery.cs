using Core.Enums;

namespace Core.Models.WorkItem
{
    public class WorkItemQuery
    {
        public int Skip { get; set; }
        public int Limit { get; set; }
        public string SearchText { get; set; }
        public WorkItemType? Type { get; set; }
        public bool? IsHighlighted { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
