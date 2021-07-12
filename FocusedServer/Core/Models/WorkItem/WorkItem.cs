using Core.Enums;
using Core.Models.Generic;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Core.Models.WorkItem
{
    [BsonIgnoreExtraElements]
    public class WorkItem : UserOwnedRecord
    {
        public List<WorkItem> Subtasks { get; set; } = new List<WorkItem>();
        public string Name { get; set; }
        public string Description { get; set; }
        public WorkItemType Type { get; set; } = WorkItemType.Regular;
        public WorkItemPriority Priority { get; set; } = WorkItemPriority.NotUrgentNotImportant;
        public WorkItemStatus Status { get; set; } = WorkItemStatus.Idle;
        public double EstimatedHours { get; set; }
        public DateTime? DueDate { get; set; }
        public bool[] Recur { get; set; } = new bool[7];
        public List<ChecklistEntry> Checklist { get; set; } = new List<ChecklistEntry>();
        public List<CompletionRecord> CompletionRecords { get; set; } = new List<CompletionRecord>();
        public TimeInfo TimeInfo { get; set; } = new TimeInfo();
    }
}
