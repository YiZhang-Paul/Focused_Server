using Core.Configurations;
using Core.Models.TimeSession;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class BreakSessionRepository : UserOwnedRecordRepository<BreakSession>
    {
        public BreakSessionRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(BreakSession).Name) { }

        public async Task<BreakSession> GetActiveBreakSession(string userId)
        {
            var now = DateTime.UtcNow;
            var builder = Builders<BreakSession>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Exists(_ => _.EndTime, false) & builder.Lte(_ => _.StartTime, now);
            var sessions = await Collection.Find(filter).ToListAsync().ConfigureAwait(false);

            return sessions.LastOrDefault(_ => _.StartTime.AddHours(_.TargetDuration) >= now);
        }

        public async Task<List<BreakSession>> GetBreakSessionByDateRange(string userId, DateTime start, DateTime end)
        {
            var builder = Builders<BreakSession>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Gte(_ => _.StartTime, start) & builder.Lte(_ => _.EndTime, end);

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }
    }
}
