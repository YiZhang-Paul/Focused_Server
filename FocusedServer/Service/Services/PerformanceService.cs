using Core.Enums;
using Core.Models.Generic;
using Service.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class PerformanceService
    {
        private const double DailyTarget = 8;
        private WorkItemRepository WorkItemRepository { get; set; }
        private FocusSessionRepository FocusSessionRepository { get; set; }

        public PerformanceService(WorkItemRepository workItemRepository, FocusSessionRepository focusSessionRepository)
        {
            WorkItemRepository = workItemRepository;
            FocusSessionRepository = focusSessionRepository;
        }

        public async Task<ProgressionCounter<double>> GetFocusProgressionByDate(int year, int month, int day)
        {
            var start = new DateTime(year, month, day);
            var end = start.AddDays(1);
            var sessions = await FocusSessionRepository.GetFocusSessionsByDateRange(start, end).ConfigureAwait(false);
            var ids = sessions.SelectMany(_ => _.WorkItemIds).Distinct().ToList();
            var progress = await WorkItemRepository.GetWorkItemProgressions(ids, start, end).ConfigureAwait(false);
            var total = progress.Sum(_ => _.ItemProgress.Current * (_.Type == WorkItemType.Interruption ? -1 : 1));

            return new ProgressionCounter<double>
            {
                Current = total,
                Target = DailyTarget,
                IsCompleted = total >= DailyTarget
            };
        }
    }
}
