using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class FocusSessionService
    {
        private FocusSessionRepository FocusSessionRepository { get; set; }

        public FocusSessionService(FocusSessionRepository focusSessionRepository)
        {
            FocusSessionRepository = focusSessionRepository;
        }

        public async Task<List<string>> GetSessionWorkItemsByDateRange(DateTime start, DateTime end)
        {
            var sessions = await FocusSessionRepository.GetFocusSessionsByDateRange(start, end).ConfigureAwait(false);

            return sessions.SelectMany(_ => _.WorkItemIds).Distinct().ToList();
        }

        public async Task<double> GetOverlearningHoursByDateRange(DateTime start, DateTime end)
        {
            var sessions = await FocusSessionRepository.GetFocusSessionsByDateRange(start, end).ConfigureAwait(false);

            return sessions.Sum(_ => _.OverlearningHours);
        }
    }
}
