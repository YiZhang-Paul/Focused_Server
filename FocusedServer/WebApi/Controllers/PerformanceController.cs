using Core.Dtos;
using Core.Interfaces.Services;
using Core.Models.Generic;
using Core.Models.User;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/performance")]
    [ApiController]
    public class PerformanceController : ControllerBase
    {
        private const string UserId = "60cd1862629e063c384f3ea1";
        private IPerformanceService PerformanceService { get; set; }

        public PerformanceController(IPerformanceService performanceService)
        {
            PerformanceService = performanceService;
        }

        [HttpGet]
        [Route("daily-progression/{year}/{month}/{day}")]
        public async Task<ProgressionCounter<double>> GetDailyProgression(int year, int month, int day)
        {
            return await PerformanceService.GetFocusProgressionByDate(UserId, year, month, day).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("daily-time-tracking/{year}/{month}/{day}")]
        public async Task<TimeTrackingBreakdownDto> GetTimeTrackingBreakdownByDate(int year, int month, int day)
        {
            return await PerformanceService.GetTimeTrackingBreakdownByDate(UserId, year, month, day).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("activity-breakdown")]
        public async Task<ActivityBreakdownDto> GetActivityBreakdownByDateRange([FromQuery]DateTime? start, [FromQuery]DateTime? end)
        {
            return await PerformanceService.GetActivityBreakdownByDateRange(UserId, start, end).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("activity-breakdown/{start}/{end}")]
        public async Task<List<ActivityBreakdownDto>> GetActivityBreakdownByDays(string start, string end)
        {
            if (!DateTime.TryParse(start, out var startDate) || !DateTime.TryParse(end, out var endDate))
            {
                return new List<ActivityBreakdownDto>();
            }

            return await PerformanceService.GetActivityBreakdownByDays(UserId, startDate, endDate).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("estimation-breakdown")]
        public async Task<EstimationBreakdownDto> GetEstimationBreakdown([FromQuery]DateTime? start, [FromQuery]DateTime? end)
        {
            return await PerformanceService.GetEstimationBreakdownByDateRange(UserId, start, end).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("due-date-breakdown")]
        public async Task<DueDateBreakdownDto> GetDueDateBreakdown([FromQuery]DateTime? start, [FromQuery]DateTime? end)
        {
            return await PerformanceService.GetDueDateBreakdownByDateRange(UserId, start, end).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("user-rating")]
        public async Task<PerformanceRating> GetPerformanceRating([FromQuery]DateTime? start, [FromQuery]DateTime? end)
        {
            return await PerformanceService.GetPerformanceRating(UserId, start, end).ConfigureAwait(false);
        }
    }
}
