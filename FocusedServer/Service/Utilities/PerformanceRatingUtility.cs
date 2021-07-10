using Core.Dtos;
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

            return (double)progression.Count(_ => WorkItemUtility.IsOverestimated(_) || _.Current > _.Target) / progression.Count;
        }

        public static double GetSustainabilityRating(List<double> durations)
        {
            var average = durations.Average();
            var variance = durations.Average(_ => Math.Pow(_ - average, 2));

            return Math.Sqrt(variance) / average;
        }
    }
}
