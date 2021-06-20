using Core.Dtos;
using Core.Models.TimeSession;
using Microsoft.AspNetCore.Mvc;
using Service.Repositories;
using Service.Services;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/time-session")]
    [ApiController]
    public class TimeSessionController : ControllerBase
    {
        private BreakSessionRepository BreakSessionRepository { get; set; }
        private FocusSessionService FocusSessionService { get; set; }

        public TimeSessionController(BreakSessionRepository breakSessionRepository, FocusSessionService focusSessionService)
        {
            BreakSessionRepository = breakSessionRepository;
            FocusSessionService = focusSessionService;
        }

        [HttpGet]
        [Route("focus-session/{id}")]
        public async Task<FocusSession> GetFocusSession(string id)
        {
            return await FocusSessionService.GetFocusSession(id).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("focus-session/{id}/activity-breakdown")]
        public async Task<ActivityBreakdownDto> GetActivityBreakdownBySession(string id)
        {
            return await FocusSessionService.GetActivityBreakdownBySession(id).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("break-session/{id}")]
        public async Task<BreakSession> GetBreakSession(string id)
        {
            return await BreakSessionRepository.Get(id).ConfigureAwait(false);
        }
    }
}
