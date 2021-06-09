using Core.Dtos;
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
            var breakdown = await GetActivityBreakdownByDateRange(start, end).ConfigureAwait(false);
            var total = breakdown.Regular + breakdown.Recurring + breakdown.Overlearning;

            return new ProgressionCounter<double>
            {
                Current = total,
                Target = DailyTarget,
                IsCompleted = total >= DailyTarget
            };
        }

        public async Task<ActivityBreakdownDto> GetActivityBreakdownByDateRange(DateTime? start, DateTime? end)
        {
            var endDate = end ?? DateTime.UtcNow;
            var startDate = start ?? endDate.AddDays(-14);
            var sessions = await FocusSessionRepository.GetFocusSessionsByDateRange(startDate, endDate).ConfigureAwait(false);
            var ids = sessions.SelectMany(_ => _.WorkItemIds).Distinct().ToList();
            var progress = await WorkItemRepository.GetWorkItemProgressions(ids, startDate, endDate).ConfigureAwait(false);

            return new ActivityBreakdownDto
            {
                Regular = progress.Sum(_ => _.Type == WorkItemType.Regular ? _.Progress.Current : 0),
                Recurring = progress.Sum(_ => _.Type == WorkItemType.Recurring ? _.Progress.Current : 0),
                Interruption = progress.Sum(_ => _.Type == WorkItemType.Interruption ? _.Progress.Current : 0),
                Overlearning = sessions.Sum(_ => _.OverlearningHours)
            };
        }

        public async Task<EstimationBreakdownDto> GetEstimationBreakdownByDateRange(DateTime? start, DateTime? end)
        {
            var endDate = end ?? DateTime.UtcNow;
            var startDate = start ?? endDate.AddDays(-14);
            var sessions = await FocusSessionRepository.GetFocusSessionsByDateRange(startDate, endDate).ConfigureAwait(false);
            var ids = sessions.SelectMany(_ => _.WorkItemIds).Distinct().ToList();
            var overallProgresses = await WorkItemRepository.GetWorkItemProgressions(ids, null, null).ConfigureAwait(false);
            var currentProgresses = await WorkItemRepository.GetWorkItemProgressions(ids, startDate, endDate).ConfigureAwait(false);
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
    }
}
