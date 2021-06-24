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
        private const string UserId = "60cd1862629e063c384f3ea1";
        private BreakSessionRepository BreakSessionRepository { get; set; }
        private FocusSessionService FocusSessionService { get; set; }

        public TimeSessionController(BreakSessionRepository breakSessionRepository, FocusSessionService focusSessionService)
        {
            BreakSessionRepository = breakSessionRepository;
            FocusSessionService = focusSessionService;
        }

        [HttpGet]
        [Route("active-focus-session/meta")]
        public async Task<FocusSessionDto> GetActiveFocusSessionMeta()
        {
            return await FocusSessionService.GetActiveFocusSessionMeta(UserId).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("active-break-session")]
        public async Task<BreakSession> GetActiveBreakSession()
        {
            return await BreakSessionRepository.GetActiveBreakSession(UserId).ConfigureAwait(false);
        }
    }
}
