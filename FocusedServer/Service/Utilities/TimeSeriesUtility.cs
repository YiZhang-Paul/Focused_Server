using Core.Models.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.Utilities
{
    public static class TimeSeriesUtility
    {
        public static double GetTotalTime<T>(List<T> series, DateTime start, DateTime end) where T : TimeRange
        {
            return series.Sum(_ =>
            {
                _.EndTime ??= DateTime.Now < end ? DateTime.Now : end;
                var rangeStart = start > _.StartTime ? start : _.StartTime;
                var rangeEnd = end < _.EndTime.Value ? end : _.EndTime.Value;

                return Math.Max(0, (rangeEnd - rangeStart).TotalHours);
            });
        }
    }
}
