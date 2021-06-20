using Core.Configurations;
using Core.Models.TimeSession;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class BreakSessionRepository : DatabaseConnector<BreakSession>
    {
        public BreakSessionRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(BreakSession).Name) { }

        public async Task<List<BreakSession>> GetBreakSessionByDateRange(string userId, DateTime start, DateTime end)
        {
            var builder = Builders<BreakSession>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Gte(_ => _.StartTime, start) & builder.Lte(_ => _.EndTime, end);

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }
    }
}
