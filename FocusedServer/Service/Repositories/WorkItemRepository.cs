using Core.Configurations;
using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Models.Aggregates;
using Core.Models.Generic;
using Core.Models.TimeSession;
using Core.Models.WorkItem;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Service.Repositories.RepositoryBase;
using Service.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class WorkItemRepository : UserOwnedRecordRepository<WorkItem>, IWorkItemRepository
    {
        public WorkItemRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(WorkItem).Name) { }

        public async Task<List<WorkItem>> GetWorkItems(string userId, WorkItemStatus status)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Eq(_ => _.Status, status);

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }

        public async Task<WorkItemDto> GetWorkItemMeta(string userId, string id)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Eq(_ => _.Id, id);
            var item = await GetWorkItemWithTimeSeriesAggregate(filter).FirstOrDefaultAsync().ConfigureAwait(false);

            return item == null ? null : WorkItemUtility.ToWorkItemDto(item);
        }

        public async Task<List<WorkItemDto>> GetWorkItemMetas(string userId, List<string> ids, DateTime? start = null, DateTime? end = null)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.In(_ => _.Id, ids);
            var items = await GetWorkItemWithTimeSeriesAggregate(filter).ToListAsync().ConfigureAwait(false);

            return items.Select(_ => WorkItemUtility.ToWorkItemDto(_, start, end)).ToList();
        }

        public async Task<List<WorkItemDto>> GetWorkItemMetas(string userId, WorkItemQuery query)
        {
            var filter = GetFilter(userId, query);
            var items = await GetWorkItemWithTimeSeriesAggregate(filter, query.Skip, query.Limit).ToListAsync().ConfigureAwait(false);

            return items.Select(_ => WorkItemUtility.ToWorkItemDto(_)).ToList();
        }

        public async Task<long> GetPastDueWorkItemsCount(string userId, DateTime start, DateTime end)
        {
            var builder = Builders<WorkItem>.Filter;

            var filter = builder.And(
                builder.Eq(_ => _.UserId, userId),
                builder.Ne(_ => _.Status, WorkItemStatus.Completed),
                builder.Gte(_ => _.DueDate, start),
                builder.Lte(_ => _.DueDate, end),
                builder.Lte(_ => _.DueDate, DateTime.Now)
            );

            return await Collection.CountDocumentsAsync(filter).ConfigureAwait(false);
        }

        public async Task<long> GetLoomingWorkItemsCount(string userId, DateTime start, DateTime end)
        {
            var builder = Builders<WorkItem>.Filter;

            var filter = builder.And(
                builder.Eq(_ => _.UserId, userId),
                builder.Ne(_ => _.Status, WorkItemStatus.Completed),
                builder.Gte(_ => _.DueDate, start),
                builder.Lte(_ => _.DueDate, end),
                builder.Gt(_ => _.DueDate, DateTime.Now)
            );

            return await Collection.CountDocumentsAsync(filter).ConfigureAwait(false);
        }

        public async Task<List<WorkItemProgressionDto>> GetWorkItemProgressionByDateRange(string userId, List<string> ids, DateTime start, DateTime end)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.In(_ => _.Id, ids);
            var items = await GetWorkItemWithTimeSeriesAggregate(filter).ToListAsync().ConfigureAwait(false);

            return items.Select(_ => new WorkItemProgressionDto
            {
                Id = _.Id,
                UserId = _.UserId,
                Type = _.Type,
                Progress = new ProgressionCounter<double>
                {
                    Current = TimeSeriesUtility.GetTotalTime(_.TimeSeries, start, end),
                    Target = _.EstimatedHours,
                    IsCompleted = _.Status == WorkItemStatus.Completed
                }
            })
            .ToList();
        }

        private FilterDefinition<WorkItem> GetFilter(string userId, WorkItemQuery query)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId);

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
            var foreignCollection = Connect<TimeSeries>(typeof(TimeSeries).Name);
            var aggregate = Collection.Aggregate().Match(filter).Skip(skip);
            aggregate = limit > 0 ? aggregate.Limit(limit) : aggregate;

            return aggregate
                .AppendStage<WorkItemWithTimeSeries>("{ $addFields: { WorkItemId: { $toString: '$_id' } } }")
                .Lookup(foreignCollection, _ => _.WorkItemId, _ => _.DataSourceId, (WorkItemWithTimeSeries _) => _.TimeSeries);
        }
    }
}
