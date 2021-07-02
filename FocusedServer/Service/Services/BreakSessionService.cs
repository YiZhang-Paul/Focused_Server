using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Service.Utilities;
using System;
using System.Threading.Tasks;

namespace Service.Services
{
    public class BreakSessionService : IBreakSessionService
    {
        private IBreakSessionRepository BreakSessionRepository { get; set; }

        public BreakSessionService(IBreakSessionRepository breakSessionRepository)
        {
            BreakSessionRepository = breakSessionRepository;
        }

        public async Task<double> GetBreakDurationByDateRange(string userId, DateTime start, DateTime end)
        {
            var sessions = await BreakSessionRepository.GetBreakSessionByDateRange(userId, start, end).ConfigureAwait(false);

            return TimeSeriesUtility.GetTotalTime(sessions, start, end);
        }
    }
}
