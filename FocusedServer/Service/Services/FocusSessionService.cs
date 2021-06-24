using Core.Dtos;
using Core.Enums;
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

        public async Task<FocusSessionDto> GetActiveFocusSessionMeta(string userId)
        {
            var session = await FocusSessionRepository.GetActiveFocusSession(userId).ConfigureAwait(false);

            if (session == null)
            {
                return null;
            }

            var progress = await WorkItemRepository.GetWorkItemProgressionByDateRange(userId, session.WorkItemIds, session.StartTime, session.EndTime).ConfigureAwait(false);

            return new FocusSessionDto
            {
                Id = session.Id,
                UserId = session.UserId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Activities = new ActivityBreakdownDto
                {
                    Regular = progress.Sum(_ => _.Type == WorkItemType.Regular ? _.Progress.Current : 0),
                    Recurring = progress.Sum(_ => _.Type == WorkItemType.Recurring ? _.Progress.Current : 0),
                    Interruption = progress.Sum(_ => _.Type == WorkItemType.Interruption ? _.Progress.Current : 0),
                    Overlearning = session.OverlearningHours
                },
                WorkItems = await WorkItemRepository.GetWorkItemMetas(userId, session.WorkItemIds).ConfigureAwait(false)
            };
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
    }
}
