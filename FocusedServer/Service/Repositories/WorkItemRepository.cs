using Core.Configurations;
using Core.Dtos;
using Core.Enums;
using Core.Models.Generic;
using Core.Models.WorkItem;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class WorkItemRepository : DatabaseConnector<WorkItem>
    {
        public WorkItemRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(WorkItem).Name) { }

        public async Task<List<WorkItemDto>> GetWorkItems(WorkItemQuery query)
        {
            return await Collection.Find(GetFilter(query))
                .Skip(query.Skip)
                .Limit(query.Limit)
                .Project(_ => new WorkItemDto
                {
                    Id = _.Id,
                    Name = _.Name,
                    Type = _.Type,
                    Priority = _.Priority,
                    Status = _.Status,
                    ItemProgress = new ProgressionCounter<double>
                    {
                        Current = GetTotalTime(_.TimeSeries, null, null),
                        Target = _.EstimatedHours,
                        IsCompleted = _.Status == WorkItemStatus.Completed
                    },
                    SubtaskProgress = new ProgressionCounter<int>
                    {
                        Current = _.Subtasks.Count(task => task.Status == WorkItemStatus.Completed),
                        Target = _.Subtasks.Count,
                        IsCompleted = _.Subtasks.All(task => task.Status == WorkItemStatus.Completed)
                    },
                    ChecklistProgress = new ProgressionCounter<int>
                    {
                        Current = _.Checklist.Count(entry => entry.IsCompleted),
                        Target = _.Checklist.Count,
                        IsCompleted = _.Checklist.All(entry => entry.IsCompleted)
                    }
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<List<WorkItemProgressionDto>> GetWorkItemProgressions(List<string> ids, DateTime? start, DateTime? end)
        {
            var filter = Builders<WorkItem>.Filter.In(_ => _.Id, ids);

            return await Collection.Find(filter)
                .Project(_ => new WorkItemProgressionDto
                {
                    Id = _.Id,
                    Type = _.Type,
                    HoursSpent = GetTotalTime(_.TimeSeries, start, end)
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        private FilterDefinition<WorkItem> GetFilter(WorkItemQuery query)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Or(builder.Eq(_ => _.Parent, null), builder.Regex(_ => _.Parent, @"^\W*$"));

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                filter &= builder.Where(_ => _.Name.ToLower().Contains(query.SearchText.ToLower()));
            }

            if (query.IsHighlighted.HasValue)
            {
                if (query.IsHighlighted.Value)
                {
                    filter &= builder.Eq(_ => _.Status, WorkItemStatus.Highlighted);
                }
                else
                {
                    filter &= builder.Ne(_ => _.Status, WorkItemStatus.Highlighted);
                }
            }

            if (query.IsCompleted.HasValue)
            {
                if (query.IsCompleted.Value)
                {
                    filter &= builder.Eq(_ => _.Status, WorkItemStatus.Completed);
                }
                else
                {
                    filter &= builder.Ne(_ => _.Status, WorkItemStatus.Completed);
                }
            }

            if (query.Type.HasValue)
            {
                filter &= builder.Eq(_ => _.Type, query.Type);
            }

            return filter;
        }

        private double GetTotalTime(TimeSeries series, DateTime? start, DateTime? end)
        {
            var total = series.ManualTracking;
            var autoTracking = series.AutoTracking.Take(series.AutoTracking.Count / 2 * 2).ToList();

            for (var i = 0; i < autoTracking.Count - 1; i += 2)
            {
                var begin = autoTracking[i];
                var stop = autoTracking[i + 1];

                if (IsValidTimeEvent(begin, TimeEventType.Begin, start, end) && IsValidTimeEvent(stop, TimeEventType.Stop, start, end))
                {
                    total += (stop.Time - begin.Time).TotalHours;
                }
            }

            if (series.AutoTracking.Any() && IsValidTimeEvent(series.AutoTracking.Last(), TimeEventType.Begin, start, end))
            {
                var now = end.HasValue && end < DateTime.UtcNow ? end.Value : DateTime.UtcNow;
                total += (now - series.AutoTracking.Last().Time).TotalHours;
            }

            return total;
        }

        private bool IsValidTimeEvent(TimeEvent timeEvent, TimeEventType type, DateTime? start, DateTime? end)
        {
            var isValidStart = !start.HasValue || timeEvent.Time >= start;
            var isValidEnd = !end.HasValue || timeEvent.Time <= end;

            return isValidStart && isValidEnd && timeEvent.Type == type;
        }
    }
}
