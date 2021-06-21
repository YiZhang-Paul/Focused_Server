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
    public class WorkItemRepository : UserOwnedRecordRepository<WorkItem>
    {
        public WorkItemRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(WorkItem).Name) { }

        public async Task<WorkItemDto> GetWorkItemMeta(string userId, string id)
        {
            return ToWorkItemDto(await Get(userId, id).ConfigureAwait(false));
        }

        public async Task<bool> UpdateWorkItemsStatus(string userId, WorkItemStatus source, WorkItemStatus target)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Eq(_ => _.Status, source);
            var update = Builders<WorkItem>.Update.Set(_ => _.Status, target);
            var result = await Collection.UpdateManyAsync(filter, update).ConfigureAwait(false);

            return result.IsAcknowledged;
        }

        public async Task<long> GetPastDueWorkItemsCount(string userId, DateTime start, DateTime end)
        {
            var builder = Builders<WorkItem>.Filter;

            var filter = builder.And(
                builder.Eq(_ => _.UserId, userId),
                builder.Gte(_ => _.DueDate, start),
                builder.Lte(_ => _.DueDate, end),
                builder.Lte(_ => _.DueDate, DateTime.UtcNow)
            );

            return await Collection.CountDocumentsAsync(filter).ConfigureAwait(false);
        }

        public async Task<long> GetLoomingWorkItemsCount(string userId, DateTime start, DateTime end)
        {
            var builder = Builders<WorkItem>.Filter;

            var filter = builder.And(
                builder.Eq(_ => _.UserId, userId),
                builder.Gte(_ => _.DueDate, start),
                builder.Lte(_ => _.DueDate, end),
                builder.Gt(_ => _.DueDate, DateTime.UtcNow)
            );

            return await Collection.CountDocumentsAsync(filter).ConfigureAwait(false);
        }

        public async Task<List<WorkItemDto>> GetWorkItemMetas(string userId, List<string> ids)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.In(_ => _.Id, ids);

            return await Collection.Find(filter).Project(_ => ToWorkItemDto(_)).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<WorkItemDto>> GetWorkItemMetas(string userId, WorkItemQuery query)
        {
            return await Collection.Find(GetFilter(userId, query))
                .Skip(query.Skip)
                .Limit(query.Limit)
                .Project(_ => ToWorkItemDto(_))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<List<WorkItemProgressionDto>> GetWorkItemProgressionByDateRange(string userId, List<string> ids, DateTime? start, DateTime? end)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.In(_ => _.Id, ids);

            return await Collection.Find(filter)
                .Project(_ => new WorkItemProgressionDto
                {
                    Id = _.Id,
                    UserId = _.UserId,
                    Type = _.Type,
                    Progress = new ProgressionCounter<double>
                    {
                        Current = GetTotalTime(_.TimeSeries, start, end),
                        Target = _.EstimatedHours,
                        IsCompleted = _.Status == WorkItemStatus.Completed
                    }
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        private FilterDefinition<WorkItem> GetFilter(string userId, WorkItemQuery query)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Or(builder.Eq(_ => _.Parent, null), builder.Regex(_ => _.Parent, @"^\W*$"));

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

        private WorkItemDto ToWorkItemDto(WorkItem item)
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
                    Current = GetTotalTime(item.TimeSeries, null, null),
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
