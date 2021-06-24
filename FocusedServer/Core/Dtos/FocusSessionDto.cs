using System;
using System.Collections.Generic;

namespace Core.Dtos
{
    public class FocusSessionDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double TargetDuration { get; set; }
        public ActivityBreakdownDto Activities { get; set; } = new ActivityBreakdownDto();
        public List<WorkItemDto> WorkItems { get; set; } = new List<WorkItemDto>();
    }
}
