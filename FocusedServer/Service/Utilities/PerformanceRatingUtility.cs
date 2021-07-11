using Core.Dtos;
using Core.Enums;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.Utilities
{
    public static class PerformanceRatingUtility
    {
        public static double GetEstimationRating(List<WorkItemProgressionDto> workItems)
        {
            var progression = workItems.Where(_ => _.Progress.IsCompleted).Select(_ => _.Progress).ToList();

            if (!progression.Any())
            {
                return 0;
            }

            return (double)progression.Count(_ => !WorkItemUtility.IsOverestimated(_) && _.Current <= _.Target) / progression.Count;
        }

        public static double GetPlanningRating(List<WorkItem> items)
        {
            var nonInterruptions = items.Where(_ => _.Type != WorkItemType.Interruption).ToList();

            if (!nonInterruptions.Any())
            {
                return 0;
            }

            return (double)nonInterruptions.Count(_ => !WorkItemUtility.IsPastDue(_)) / nonInterruptions.Count;
        }

        public static double GetAdaptabilityRating(List<WorkItem> items)
        {
            var interruptions = items.Where(_ => _.Type == WorkItemType.Interruption).ToList();

            if (!interruptions.Any())
            {
                return 0;
            }

            return (double)interruptions.Count(_ => !WorkItemUtility.IsPastDue(_)) / interruptions.Count;
        }

        public static double GetSustainabilityRating(List<double> durations)
        {
            var average = durations.Average();
            var variance = durations.Average(_ => Math.Pow(_ - average, 2));

            return average / (average + Math.Sqrt(variance));
        }
    }
}
