using Core.Dtos;
using Core.Models.Generic;
using Microsoft.AspNetCore.Mvc;
using Service.Services;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/performance")]
    [ApiController]
    public class PerformanceController : ControllerBase
    {
        private PerformanceService PerformanceService { get; set; }

        public PerformanceController(PerformanceService performanceService)
        {
            PerformanceService = performanceService;
        }

        [HttpGet]
        [Route("daily-progression/{year}/{month}/{day}")]
        public async Task<ProgressionCounter<double>> GetDailyProgression(int year, int month, int day)
        {
            return await PerformanceService.GetFocusProgressionByDate(year, month, day).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("activity-breakdown")]
        public async Task<ActivityBreakdownDto> GetActivityBreakdown([FromQuery]DateTime? start, [FromQuery]DateTime? end)
        {
            return await PerformanceService.GetActivityBreakdownByDateRange(start, end).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("estimation-breakdown")]
        public async Task<EstimationBreakdownDto> GetEstimationBreakdown([FromQuery]DateTime? start, [FromQuery]DateTime? end)
        {
            return await PerformanceService.GetEstimationBreakdownByDateRange(start, end).ConfigureAwait(false);
        }
    }
}
