using Core.Configurations;
using Core.Dtos;
using Core.Enums;
using Core.Models.Aggregates;
using Core.Models.Generic;
using Core.Models.TimeSession;
using Core.Models.WorkItem;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Service.Utilities;
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
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Eq(_ => _.Id, id);
            var aggregate = GetWorkItemWithTimeSeriesAggregate(filter);

            return await aggregate.Project(_ => WorkItemUtility.ToWorkItemDto(_)).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<List<WorkItemDto>> GetWorkItemMetas(string userId, List<string> ids)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.In(_ => _.Id, ids);
            var aggregate = GetWorkItemWithTimeSeriesAggregate(filter);

            return await aggregate.Project(_ => WorkItemUtility.ToWorkItemDto(_)).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<WorkItemDto>> GetWorkItemMetas(string userId, WorkItemQuery query)
        {
            var aggregate = GetWorkItemWithTimeSeriesAggregate(GetFilter(userId, query), query.Skip, query.Limit);

            return await aggregate.Project(_ => WorkItemUtility.ToWorkItemDto(_)).ToListAsync().ConfigureAwait(false);
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

        public async Task<List<WorkItemProgressionDto>> GetWorkItemProgressionByDateRange(string userId, List<string> ids, DateTime start, DateTime end)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.In(_ => _.Id, ids);

            return await GetWorkItemWithTimeSeriesAggregate(filter)
                .Project(_ => new WorkItemProgressionDto
                {
                    Id = _.Id,
                    UserId = _.UserId,
                    Type = _.Type,
                    Progress = new ProgressionCounter<double>
                    {
                        Current = WorkItemUtility.GetTotalTime(_.TimeSeries, start, end),
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

        private IAggregateFluent<WorkItemWithTimeSeries> GetWorkItemWithTimeSeriesAggregate(FilterDefinition<WorkItem> filter, int skip = 0, int limit = 0)
        {
            var aggregate = Collection.Aggregate().Match(filter).Skip(skip).Limit(limit);
            var foreignCollection = Connect<TimeSeries>(typeof(TimeSeries).Name);

            return aggregate.Lookup(foreignCollection, _ => _.Id, _ => _.DataSourceId, (WorkItemWithTimeSeries _) => _.TimeSeries);
        }
    }
}
