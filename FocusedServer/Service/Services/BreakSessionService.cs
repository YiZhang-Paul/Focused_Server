using Service.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class BreakSessionService
    {
        private BreakSessionRepository BreakSessionRepository { get; set; }

        public BreakSessionService(BreakSessionRepository breakSessionRepository)
        {
            BreakSessionRepository = breakSessionRepository;
        }

        public async Task<double> GetBreakDurationByDateRange(string userId, DateTime start, DateTime end)
        {
            var sessions = await BreakSessionRepository.GetBreakSessionByDateRange(userId, start, end).ConfigureAwait(false);

            return sessions.Sum(_ => (_.EndTime - _.StartTime).TotalHours);
        }
    }
}
