using Core.Dtos;
using Core.Enums;
using Core.Models.TimeSession;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class FocusSessionService
    {
        private WorkItemRepository WorkItemRepository { get; set; }
        private FocusSessionRepository FocusSessionRepository { get; set; }

        public FocusSessionService(WorkItemRepository workItemRepository, FocusSessionRepository focusSessionRepository)
        {
            WorkItemRepository = workItemRepository;
            FocusSessionRepository = focusSessionRepository;
        }

        public async Task<FocusSession> GetFocusSession(string userId, string id)
        {
            return await FocusSessionRepository.Get(userId, id).ConfigureAwait(false);
        }

        public async Task<List<string>> GetSessionWorkItemsByDateRange(string userId, DateTime start, DateTime end)
        {
            var sessions = await FocusSessionRepository.GetFocusSessionsByDateRange(userId, start, end).ConfigureAwait(false);

            return sessions.SelectMany(_ => _.WorkItemIds).Distinct().ToList();
        }

        public async Task<double> GetOverlearningHoursByDateRange(string userId, DateTime start, DateTime end)
        {
            var sessions = await FocusSessionRepository.GetFocusSessionsByDateRange(userId, start, end).ConfigureAwait(false);

            return sessions.Sum(_ => _.OverlearningHours);
        }

        public async Task<ActivityBreakdownDto> GetActivityBreakdownBySession(string userId, string id)
        {
            var session = await GetFocusSession(userId, id).ConfigureAwait(false);

            if (session == null)
            {
                return new ActivityBreakdownDto();
            }

            var ids = session.WorkItemIds;
            var start = session.StartTime;
            var end = session.EndTime;
            var progress = await WorkItemRepository.GetWorkItemProgressionByDateRange(session.UserId, ids, start, end).ConfigureAwait(false);

            return new ActivityBreakdownDto
            {
                Regular = progress.Sum(_ => _.Type == WorkItemType.Regular ? _.Progress.Current : 0),
                Recurring = progress.Sum(_ => _.Type == WorkItemType.Recurring ? _.Progress.Current : 0),
                Interruption = progress.Sum(_ => _.Type == WorkItemType.Interruption ? _.Progress.Current : 0),
                Overlearning = session.OverlearningHours
            };
        }
    }
}
