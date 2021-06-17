using Core.Dtos;
using Core.Enums;
using Core.Models.Generic;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class PerformanceService
    {
        private const double DailyTarget = 8;
        private const double DefaultPeriod = 14;
        private WorkItemRepository WorkItemRepository { get; set; }
        private FocusSessionService FocusSessionService { get; set; }

        public PerformanceService(WorkItemRepository workItemRepository, FocusSessionService focusSessionService)
        {
            WorkItemRepository = workItemRepository;
            FocusSessionService = focusSessionService;
        }

        public async Task<ProgressionCounter<double>> GetFocusProgressionByDate(int year, int month, int day)
        {
            var start = new DateTime(year, month, day);
            var end = start.AddDays(1);
            var breakdown = await GetActivityBreakdownByDateRange(start, end).ConfigureAwait(false);
            var total = breakdown.Regular + breakdown.Recurring + breakdown.Overlearning;

            return new ProgressionCounter<double>
            {
                Current = total,
                Target = DailyTarget,
                IsCompleted = total >= DailyTarget
            };
        }

        public async Task<List<ActivityBreakdownDto>> GetActivityBreakdownByDays(DateTime start, DateTime end)
        {
            if (start.Date > end.Date)
            {
                return new List<ActivityBreakdownDto>();
            }

            var total = (end.Date - start.Date).Days;
            var days = Enumerable.Range(0, total).Select(_ => start.Date.AddDays(_));
            var tasks = days.Select(async _ => await GetActivityBreakdownByDateRange(_, _.AddDays(1)).ConfigureAwait(false));

            return (await Task.WhenAll(tasks).ConfigureAwait(false)).ToList();
        }

        public async Task<ActivityBreakdownDto> GetActivityBreakdownByDateRange(DateTime? start, DateTime? end)
        {
            var endDate = end ?? DateTime.UtcNow;
            var startDate = start ?? endDate.AddDays(-DefaultPeriod);
            var ids = await FocusSessionService.GetSessionWorkItemsByDateRange(startDate, endDate).ConfigureAwait(false);
            var progress = await WorkItemRepository.GetWorkItemProgressionByDateRange(ids, startDate, endDate).ConfigureAwait(false);

            return new ActivityBreakdownDto
            {
                Regular = progress.Sum(_ => _.Type == WorkItemType.Regular ? _.Progress.Current : 0),
                Recurring = progress.Sum(_ => _.Type == WorkItemType.Recurring ? _.Progress.Current : 0),
                Interruption = progress.Sum(_ => _.Type == WorkItemType.Interruption ? _.Progress.Current : 0),
                Overlearning = await FocusSessionService.GetOverlearningHoursByDateRange(startDate, endDate).ConfigureAwait(false)
            };
        }

        public async Task<EstimationBreakdownDto> GetEstimationBreakdownByDateRange(DateTime? start, DateTime? end)
        {
            var endDate = end ?? DateTime.UtcNow;
            var startDate = start ?? endDate.AddDays(-DefaultPeriod);
            var ids = await FocusSessionService.GetSessionWorkItemsByDateRange(startDate, endDate).ConfigureAwait(false);
            var currentProgresses = await WorkItemRepository.GetWorkItemProgressionByDateRange(ids, startDate, endDate).ConfigureAwait(false);
            var overallProgresses = await WorkItemRepository.GetWorkItemProgressionByDateRange(ids, null, null).ConfigureAwait(false);
            var overallLookup = overallProgresses.ToDictionary(_ => _.Id);
            var breakdown = new EstimationBreakdownDto();

            foreach (var progression in currentProgresses)
            {
                var overallProgress = overallLookup[progression.Id].Progress;
                var currentProgress = progression.Progress;

                if (overallProgress.Current >= overallProgress.Target)
                {
                    var underestimate = Math.Min(currentProgress.Current, overallProgress.Current - overallProgress.Target);
                    breakdown.Normal += currentProgress.Current - underestimate;
                    breakdown.Underestimate += underestimate;

                    continue;
                }

                var remaining = overallProgress.Target - overallProgress.Current;
                var remainingPercentage = remaining / overallProgress.Target;
                var isOverestimate = remaining > 3 || (overallProgress.Target > 0.5 && remainingPercentage >= 0.6);
                breakdown.Normal += currentProgress.Current;
                breakdown.Overestimate += overallProgress.IsCompleted && isOverestimate ? remaining : 0;
            }

            return breakdown;
        }

        public async Task<DueDateBreakdownDto> GetDueDateBreakdownByDateRange(DateTime? start, DateTime? end)
        {
            var endDate = end ?? DateTime.UtcNow;
            var startDate = start ?? endDate.AddDays(-DefaultPeriod);

            return new DueDateBreakdownDto
            {
                PastDue = (int)await WorkItemRepository.GetPastDueWorkItemsCount(startDate, endDate).ConfigureAwait(false),
                Looming = (int)await WorkItemRepository.GetLoomingWorkItemsCount(startDate, endDate.AddDays(1)).ConfigureAwait(false)
            };
        }
    }
}
