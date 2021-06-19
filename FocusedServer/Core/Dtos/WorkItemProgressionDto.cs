using Core.Enums;
using Core.Models.Generic;

namespace Core.Dtos
{
    public class WorkItemProgressionDto
    {
        public string Id { get; set; }
        public WorkItemType Type { get; set; }
        public ProgressionCounter<double> Progress { get; set; }
    }
}
