using Core.Dtos;

namespace Core.Models.TimeSession
{
    public class FocusSessionStartupOption
    {
        public int TotalMinutes { get; set; }
        public WorkItemDto StartingItem { get; set; }
    }
}
