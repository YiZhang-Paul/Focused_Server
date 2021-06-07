using Core.Configurations;
using Core.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class FocusSessionRepository : DatabaseConnector<FocusSession>
    {
        public FocusSessionRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(FocusSession).Name) { }

        public async Task<List<FocusSession>> GetFocusSessionsByDateRange(DateTime start, DateTime end)
        {
            var builder = Builders<FocusSession>.Filter;
            var filter = builder.Gte(_ => _.StartTime, start) & builder.Lte(_ => _.EndTime, end);

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }
    }
}
