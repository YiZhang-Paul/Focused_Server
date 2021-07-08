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
        public static bool IsOverestimated(ProgressionCounter<double> progress)
        {
            if (!progress.IsCompleted)
            {
                return false;
            }

            var remaining = progress.Target - progress.Current;

            if (remaining > 3)
            {
                return true;
            }

            return progress.Target > 0.5 && remaining / progress.Target >= 0.6;
        }

        public static WorkItemDto ToWorkItemDto(WorkItemWithTimeSeries item, DateTime? start = null, DateTime? end = null)
        {
            var startTime = start ?? item.TimeInfo.Created;
            var endTime = end ?? DateTime.Now;

            return new WorkItemDto
            {
                Id = item.Id,
                UserId = item.UserId,
                Name = item.Name,
                Description = item.Description,
                Type = item.Type,
                Priority = item.Priority,
                Status = item.Status,
                DueDate = item.DueDate,
                ItemProgress = new ProgressionCounter<double>
                {
                    Current = TimeSeriesUtility.GetTotalTime(item.TimeSeries, startTime, endTime),
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
