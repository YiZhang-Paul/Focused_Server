using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.TimeSession;
using Service.Utilities;
using System;
using System.Threading.Tasks;

namespace Service.Services
{
    public class FocusSessionService : IFocusSessionService
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
            return await GetOpenFocusSessionMeta(userId, false).ConfigureAwait(false);
        }

        public async Task<FocusSessionDto> GetStaleFocusSessionMeta(string userId)
        {
            return await GetOpenFocusSessionMeta(userId, true).ConfigureAwait(false);
        }

        public async Task<bool> StartFocusSession(string userId, FocusSessionStartupOption option)
        {
            if (option.StartingItem == null || await FocusSessionRepository.GetUnfinishedFocusSession(userId).ConfigureAwait(false) != null)
            {
                return false;
            }

            var session = new FocusSession
            {
                UserId = userId,
                StartTime = DateTime.Now,
                TargetDuration = (double)option.TotalMinutes / 60
            };

            var id = await FocusSessionRepository.Add(session).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return await WorkItemService.StartWorkItem(userId, option.StartingItem.Id).ConfigureAwait(false);
        }

        public async Task<bool> StopFocusSession(string userId, string id)
        {
            var session = await FocusSessionRepository.Get(userId, id).ConfigureAwait(false);

            if (session == null)
            {
                return false;
            }

            session.EndTime = session.TargetEndTime < DateTime.Now ? session.TargetEndTime : DateTime.Now;

            if(await FocusSessionRepository.Replace(session).ConfigureAwait(false) == null)
            {
                return false;
            }

            return await WorkItemService.StopWorkItem(userId).ConfigureAwait(false);
        }

        public async Task<double> GetOverlearningHoursByDateRange(string userId, DateTime start, DateTime end)
        {
            var series = await TimeSeriesRepository.GetTimeSeriesByDateRange(userId, start, end, TimeSeriesType.Session);

            return TimeSeriesUtility.GetTotalTime(series, start, end);
        }

        private async Task<FocusSessionDto> GetOpenFocusSessionMeta(string userId, bool isStale)
        {
            var session = isStale ?
                await FocusSessionRepository.GetStaleFocusSession(userId).ConfigureAwait(false) :
                await FocusSessionRepository.GetUnfinishedFocusSession(userId).ConfigureAwait(false);

            if (session == null)
            {
                return null;
            }

            var end = isStale ? session.TargetEndTime : DateTime.Now;
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
    }
}
