using Core.Configurations;
using Core.Enums;
using Core.Models.TimeSession;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Service.Repositories.RepositoryBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class TimeSeriesRepository : TimeRangeRecordRepository<TimeSeries>
    {
        public TimeSeriesRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(TimeSeries).Name) { }

        public async Task<List<string>> GetDataSourceIdsByDateRange(string userId, DateTime start, DateTime end, TimeSeriesType? type)
        {
            var series = await GetTimeSeriesByDateRange(userId, start, end, type).ConfigureAwait(false);

            return series.Select(_ => _.DataSourceId).Distinct().ToList();
        }

        public async Task<List<TimeSeries>> GetTimeSeriesByDataSource(string userId, string dataSourceId)
        {
            var builder = Builders<TimeSeries>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Eq(_ => _.DataSourceId, dataSourceId);

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<TimeSeries>> GetTimeSeriesByDateRange(string userId, DateTime start, DateTime end, TimeSeriesType? type)
        {
            var filter = GetOverlappingTimeRangeFilter(userId, start, end);

            if (type.HasValue)
            {
                filter &= Builders<TimeSeries>.Filter.Eq(_ => _.Type, type);
            }

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }
    }
}
