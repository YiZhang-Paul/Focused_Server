using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Service.Utilities;
using System;
using System.Threading.Tasks;

namespace Service.Services
{
    public class FocusSessionService
    {
        private IWorkItemRepository WorkItemRepository { get; set; }
        private ITimeSeriesRepository TimeSeriesRepository { get; set; }
        private IFocusSessionRepository FocusSessionRepository { get; set; }
        private IWorkItemService WorkItemService { get; set; }

        public FocusSessionService
        (
            IWorkItemRepository workItemRepository,
            ITimeSeriesRepository timeSeriesRepository,
            IFocusSessionRepository focusSessionRepository,
            IWorkItemService workItemService
        )
        {
            WorkItemRepository = workItemRepository;
            TimeSeriesRepository = timeSeriesRepository;
            FocusSessionRepository = focusSessionRepository;
            WorkItemService = workItemService;
        }

        public async Task<FocusSessionDto> GetActiveFocusSessionMeta(string userId)
        {
            var session = await FocusSessionRepository.GetActiveFocusSession(userId).ConfigureAwait(false);

            if (session == null)
            {
                return null;
            }

            var end = session.EndTime ?? DateTime.UtcNow;
            var ids = await TimeSeriesRepository.GetDataSourceIdsByDateRange(userId, session.StartTime, end, TimeSeriesType.WorkItem).ConfigureAwait(false);
            var progress = await WorkItemService.GetWorkItemActivityBreakdownByDateRange(userId, session.StartTime, end).ConfigureAwait(false);
            progress.Overlearning = await GetOverlearningHoursByDateRange(userId, session.StartTime, end).ConfigureAwait(false);

            return new FocusSessionDto
            {
                Id = session.Id,
                UserId = session.UserId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                TargetDuration = session.TargetDuration,
                Activities = progress,
                WorkItems = await WorkItemRepository.GetWorkItemMetas(userId, ids).ConfigureAwait(false)
            };
        }

        public async Task<double> GetOverlearningHoursByDateRange(string userId, DateTime start, DateTime end)
        {
            var series = await TimeSeriesRepository.GetTimeSeriesByDateRange(userId, start, end, TimeSeriesType.Session);

            return TimeSeriesUtility.GetTotalTime(series, start, end);
        }
    }
}
