using Core.Configurations;
using Core.Models.TimeSession;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class TimeSeriesRepository : UserOwnedRecordRepository<TimeSeries>
    {
        public TimeSeriesRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(TimeSeries).Name) { }

        public async Task<List<TimeSeries>> GetTimeSeriesByDataSource(string userId, string dataSourceId)
        {
            var builder = Builders<TimeSeries>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Eq(_ => _.DataSourceId, dataSourceId);

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<TimeSeries>> GetTimeSeriesByDataSources(string userId, List<string> dataSourceIds)
        {
            var builder = Builders<TimeSeries>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.In(_ => _.DataSourceId, dataSourceIds);

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }
    }
}
