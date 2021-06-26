using Core.Configurations;
using Core.Models.Generic;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Repositories.RepositoryBase
{
    public class TimeRangeRecordRepository<T> : UserOwnedRecordRepository<T> where T : TimeRange
    {
        public TimeRangeRecordRepository(IOptions<DatabaseConfiguration> configuration, string collection) : base(configuration, collection) { }

        public async Task<List<T>> GetOpenTimeRange(string userId)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Exists(_ => _.EndTime, false);

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }

        protected FilterDefinition<T> GetOverlappingTimeRangeFilter(string userId, DateTime start, DateTime end)
        {
            var builder = Builders<T>.Filter;
            var rangeFilter = builder.Gte(_ => _.StartTime, start) & builder.Lte(_ => _.EndTime, end);

            var startTimeFilter = builder.And(
                builder.Exists(_ => _.EndTime, false) | builder.Gte(_ => _.EndTime, start),
                builder.Lte(_ => _.StartTime, start)
            );

            var endTimeFilter = builder.And(
                builder.Exists(_ => _.EndTime, false) | builder.Gte(_ => _.EndTime, end),
                builder.Lte(_ => _.StartTime, end)
            );

            return builder.And(
                builder.Eq(_ => _.UserId, userId),
                builder.Or(rangeFilter, startTimeFilter, endTimeFilter)
            );
        }
    }
}
