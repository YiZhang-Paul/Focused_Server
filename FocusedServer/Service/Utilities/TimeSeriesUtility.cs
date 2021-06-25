using Core.Models.TimeSession;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.Utilities
{
    public static class TimeSeriesUtility
    {
        public static double GetTotalTime(List<TimeSeries> series, DateTime start, DateTime end)
        {
            return series.Aggregate(0d, (total, record) =>
            {
                record.EndTime ??= DateTime.UtcNow < end ? DateTime.UtcNow : end;
                var rangeStart = start > record.StartTime ? start : record.StartTime;
                var rangeEnd = end < record.EndTime ? end : record.EndTime.Value;

                return total + Math.Max(0, (rangeEnd - rangeStart).TotalHours);
            });
        }
    }
}
