using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.TimeSession;
using Service.Utilities;
using System;
using System.Threading.Tasks;

namespace Service.Services
{
    public class BreakSessionService : IBreakSessionService
    {
        private IFocusSessionRepository FocusSessionRepository { get; set; }
        private IBreakSessionRepository BreakSessionRepository { get; set; }

        public BreakSessionService(IFocusSessionRepository focusSessionRepository, IBreakSessionRepository breakSessionRepository)
        {
            FocusSessionRepository = focusSessionRepository;
            BreakSessionRepository = breakSessionRepository;
        }

        public async Task<double> GetBreakDurationByDateRange(string userId, DateTime start, DateTime end)
        {
            var sessions = await BreakSessionRepository.GetBreakSessionByDateRange(userId, start, end).ConfigureAwait(false);

            return TimeSeriesUtility.GetTotalTime(sessions, start, end);
        }

        public async Task<bool> StartBreakSession(string userId, BreakSessionStartupOption option)
        {
            if (string.IsNullOrWhiteSpace(option.FocusSessionId))
            {
                return false;
            }

            var focusSession = await FocusSessionRepository.Get(userId, option.FocusSessionId).ConfigureAwait(false);

            if (focusSession?.EndTime == null || await BreakSessionRepository.GetActiveBreakSession(userId).ConfigureAwait(false) != null)
            {
                return false;
            }

            var session = new BreakSession
            {
                UserId = userId,
                StartTime = DateTime.Now,
                FocusSessionId = option.FocusSessionId,
                TargetDuration = (double)option.TotalMinutes / 60
            };

            var id = await BreakSessionRepository.Add(session).ConfigureAwait(false);

            return !string.IsNullOrWhiteSpace(id);
        }

        public async Task<bool> StopBreakSession(string userId, string id)
        {
            var session = await BreakSessionRepository.Get(userId, id).ConfigureAwait(false);

            if (session == null)
            {
                return false;
            }

            session.EndTime = DateTime.Now;

            return await BreakSessionRepository.Replace(session).ConfigureAwait(false) != null;
        }
    }
}
