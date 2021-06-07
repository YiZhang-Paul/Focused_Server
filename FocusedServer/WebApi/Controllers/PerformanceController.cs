using Core.Dtos;
using Core.Models.Generic;
using Microsoft.AspNetCore.Mvc;
using Service.Services;
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
        [Route("daily-activity-breakdown/{year}/{month}/{day}")]
        public async Task<ActivityBreakdownDto> GetDailyActivityBreakdown(int year, int month, int day)
        {
            return await PerformanceService.GetActivityBreakdownByDate(year, month, day).ConfigureAwait(false);
        }
    }
}
