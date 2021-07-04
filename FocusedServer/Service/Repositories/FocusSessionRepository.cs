using Core.Configurations;
using Core.Interfaces.Repositories;
using Core.Models.TimeSession;
using Microsoft.Extensions.Options;
using Service.Repositories.RepositoryBase;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class FocusSessionRepository : TimeRangeRecordRepository<FocusSession>, IFocusSessionRepository
    {
        public FocusSessionRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(FocusSession).Name) { }

        public async Task<FocusSession> GetUnfinishedFocusSession(string userId)
        {
            var sessions = await GetOpenTimeRange(userId).ConfigureAwait(false);

            return sessions.LastOrDefault(_ => _.StartTime.AddHours(_.TargetDuration) > DateTime.Now);
        }
    }
}
