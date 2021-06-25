using Core.Dtos;
using Core.Enums;
using Core.Models.Aggregates;
using Core.Models.Generic;
using System;
using System.Linq;

namespace Service.Utilities
{
    public static class WorkItemUtility
    {
        public static WorkItemDto ToWorkItemDto(WorkItemWithTimeSeries item)
        {
            return new WorkItemDto
            {
                Id = item.Id,
                UserId = item.UserId,
                Name = item.Name,
                Type = item.Type,
                Priority = item.Priority,
                Status = item.Status,
                DueDate = item.DueDate,
                ItemProgress = new ProgressionCounter<double>
                {
                    Current = TimeSeriesUtility.GetTotalTime(item.TimeSeries, item.TimeInfo.Created, DateTime.UtcNow),
                    Target = item.EstimatedHours,
                    IsCompleted = item.Status == WorkItemStatus.Completed
                },
                SubtaskProgress = new ProgressionCounter<int>
                {
                    Current = item.Subtasks.Count(_ => _.Status == WorkItemStatus.Completed),
                    Target = item.Subtasks.Count,
                    IsCompleted = item.Subtasks.All(_ => _.Status == WorkItemStatus.Completed)
                },
                ChecklistProgress = new ProgressionCounter<int>
                {
                    Current = item.Checklist.Count(_ => _.IsCompleted),
                    Target = item.Checklist.Count,
                    IsCompleted = item.Checklist.All(_ => _.IsCompleted)
                }
            };
        }
    }
}
