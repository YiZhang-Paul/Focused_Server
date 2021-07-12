using Core.Configurations;
using Core.Interfaces.Repositories;
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
    public class BreakSessionRepository : TimeRangeRecordRepository<BreakSession>, IBreakSessionRepository
    {
        public BreakSessionRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(BreakSession).Name) { }

        public async Task<BreakSession> GetUnfinishedBreakSession(string userId)
        {
            var sessions = await GetOpenTimeRange(userId).ConfigureAwait(false);

            return sessions.LastOrDefault(_ => _.TargetEndTime > DateTime.Now);
        }

        public async Task<BreakSession> GetStaleBreakSession(string userId)
        {
            var sessions = await GetOpenTimeRange(userId).ConfigureAwait(false);

            return sessions.LastOrDefault(_ => _.TargetEndTime <= DateTime.Now);
        }

        public async Task<List<BreakSession>> GetBreakSessionByDateRange(string userId, DateTime start, DateTime end)
        {
            var filter = GetOverlappingTimeRangeFilter(userId, start, end);

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }
    }
}
