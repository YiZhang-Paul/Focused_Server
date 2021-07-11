using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Generic;
using Core.Models.User;
using Service.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class PerformanceService : IPerformanceService
    {
        private const double DailyTarget = 8;
        private const double DefaultPeriod = 14;
        private IWorkItemRepository WorkItemRepository { get; set; }
        private IWorkItemService WorkItemService { get; set; }
        private IFocusSessionService FocusSessionService { get; set; }
        private IBreakSessionService BreakSessionService { get; set; }

        public PerformanceService
        (
            IWorkItemRepository workItemRepository,
            IWorkItemService workItemService,
            IFocusSessionService focusSessionService,
            IBreakSessionService breakSessionService
        )
        {
            WorkItemRepository = workItemRepository;
            WorkItemService = workItemService;
            FocusSessionService = focusSessionService;
            BreakSessionService = breakSessionService;
        }

        public async Task<ProgressionCounter<double>> GetFocusProgressionByDate(string userId, int year, int month, int day)
        {
            var start = new DateTime(year, month, day);
            var end = start.AddDays(1);
            var breakdown = await GetActivityBreakdownByDateRange(userId, start, end).ConfigureAwait(false);
            var total = breakdown.Regular + breakdown.Recurring + breakdown.Overlearning;

            return new ProgressionCounter<double>
            {
                Current = total,
                Target = DailyTarget,
                IsCompleted = total >= DailyTarget
            };
        }

        public async Task<TimeTrackingBreakdownDto> GetTimeTrackingBreakdownByDate(string userId, int year, int month, int day)
        {
            var start = new DateTime(year, month, day);
            var end = start.AddDays(1);
            var breakdown = await GetActivityBreakdownByDateRange(userId, start, end).ConfigureAwait(false);
            var activityTime = breakdown.Regular + breakdown.Recurring + breakdown.Interruption + breakdown.Overlearning;
            var breakTime = await BreakSessionService.GetBreakDurationByDateRange(userId, start, end).ConfigureAwait(false);

            return new TimeTrackingBreakdownDto
            {
                ActivityTime = activityTime,
                BreakTime = breakTime,
                UntrackedTime = 24 - activityTime - breakTime
            };
        }

        public async Task<List<ActivityBreakdownDto>> GetActivityBreakdownByDays(string userId, DateTime start, DateTime end)
        {
            if (start.Date > end.Date)
            {
                return new List<ActivityBreakdownDto>();
            }

            var total = (end.Date - start.Date).Days;
            var days = Enumerable.Range(0, total).Select(_ => start.Date.AddDays(_));
            var tasks = days.Select(async _ => await GetActivityBreakdownByDateRange(userId, _, _.AddDays(1)).ConfigureAwait(false));

            return (await Task.WhenAll(tasks).ConfigureAwait(false)).ToList();
        }

        public async Task<ActivityBreakdownDto> GetActivityBreakdownByDateRange(string userId, DateTime? start, DateTime? end)
        {
            var endDate = end ?? DateTime.Now;
            var startDate = start ?? endDate.AddDays(-DefaultPeriod);
            var progress = await WorkItemService.GetWorkItemActivityBreakdownByDateRange(userId, startDate, endDate).ConfigureAwait(false);
            progress.Overlearning = await FocusSessionService.GetOverlearningHoursByDateRange(userId, startDate, endDate).ConfigureAwait(false);

            return progress;
        }

        public async Task<EstimationBreakdownDto> GetEstimationBreakdownByDateRange(string userId, DateTime? start, DateTime? end)
        {
            var endDate = end ?? DateTime.Now;
            var startDate = start ?? endDate.AddDays(-DefaultPeriod);
            var currentProgresses = await WorkItemService.GetWorkItemCurrentProgressionByDateRange(userId, startDate, endDate).ConfigureAwait(false);
            var overallProgresses = await WorkItemService.GetWorkItemOverallProgressionByDateRange(userId, startDate, endDate).ConfigureAwait(false);
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

                breakdown.Normal += currentProgress.Current;
                breakdown.Overestimate += WorkItemUtility.IsOverestimated(overallProgress) ? overallProgress.Target - overallProgress.Current : 0;
            }

            return breakdown;
        }

        public async Task<DueDateBreakdownDto> GetDueDateBreakdownByDateRange(string userId, DateTime? start, DateTime? end)
        {
            var endDate = end ?? DateTime.Now;
            var startDate = start ?? new DateTime(1970, 1, 1);

            return new DueDateBreakdownDto
            {
                PastDue = (int)await WorkItemRepository.GetUncompletedPastDueWorkItemsCount(userId, startDate, endDate).ConfigureAwait(false),
                Looming = (int)await WorkItemRepository.GetLoomingWorkItemsCount(userId, startDate, endDate.AddDays(1)).ConfigureAwait(false)
            };
        }

        public async Task<PerformanceRating> GetPerformanceRating(string userId, DateTime? start, DateTime? end)
        {
            var endDate = end ?? DateTime.Now;
            var startDate = start ?? endDate.AddDays(-DefaultPeriod);
            var workItems = await WorkItemService.GetWorkItemsByDateRange(userId, startDate, endDate).ConfigureAwait(false);
            var progresses = await WorkItemService.GetWorkItemOverallProgressionByDateRange(userId, startDate, endDate).ConfigureAwait(false);
            var breakdowns = await GetActivityBreakdownByDays(userId, startDate, endDate).ConfigureAwait(false);
            var durations = breakdowns.Select(_ => _.Regular + _.Recurring + _.Overlearning).ToList();

            return new PerformanceRating
            {
                Determination = (double)durations.Count(_ => _ >= DailyTarget) / durations.Count,
                Estimation = PerformanceRatingUtility.GetEstimationRating(progresses),
                Planning = PerformanceRatingUtility.GetPlanningRating(workItems),
                Adaptability = PerformanceRatingUtility.GetAdaptabilityRating(workItems),
                Sustainability = PerformanceRatingUtility.GetSustainabilityRating(durations)
            };
        }
    }
}
