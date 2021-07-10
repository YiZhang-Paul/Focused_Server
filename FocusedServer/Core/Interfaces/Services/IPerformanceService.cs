using Core.Dtos;
using Core.Models.Generic;
using Core.Models.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IPerformanceService
    {
        Task<ProgressionCounter<double>> GetFocusProgressionByDate(string userId, int year, int month, int day);
        Task<TimeTrackingBreakdownDto> GetTimeTrackingBreakdownByDate(string userId, int year, int month, int day);
        Task<List<ActivityBreakdownDto>> GetActivityBreakdownByDays(string userId, DateTime start, DateTime end);
        Task<ActivityBreakdownDto> GetActivityBreakdownByDateRange(string userId, DateTime? start, DateTime? end);
        Task<EstimationBreakdownDto> GetEstimationBreakdownByDateRange(string userId, DateTime? start, DateTime? end);
        Task<DueDateBreakdownDto> GetDueDateBreakdownByDateRange(string userId, DateTime? start, DateTime? end);
        Task<PerformanceRating> GetPerformanceRating(string userId, DateTime? start, DateTime? end);
    }
}
