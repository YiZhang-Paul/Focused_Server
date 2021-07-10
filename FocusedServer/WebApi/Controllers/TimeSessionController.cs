using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.TimeSession;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/time-session")]
    [ApiController]
    public class TimeSessionController : ControllerBase
    {
        private const string UserId = "60cd1862629e063c384f3ea1";
        private IBreakSessionRepository BreakSessionRepository { get; set; }
        private IBreakSessionService BreakSessionService { get; set; }
        private IFocusSessionService FocusSessionService { get; set; }

        public TimeSessionController
        (
            IBreakSessionRepository breakSessionRepository,
            IBreakSessionService breakSessionService,
            IFocusSessionService focusSessionService
        )
        {
            BreakSessionRepository = breakSessionRepository;
            BreakSessionService = breakSessionService;
            FocusSessionService = focusSessionService;
        }

        [HttpGet]
        [Route("active-focus-session/meta")]
        public async Task<FocusSessionDto> GetActiveFocusSessionMeta()
        {
            return await FocusSessionService.GetActiveFocusSessionMeta(UserId).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("stale-focus-session/meta")]
        public async Task<FocusSessionDto> GetStaleFocusSessionMeta()
        {
            return await FocusSessionService.GetStaleFocusSessionMeta(UserId).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("focus-session/start")]
        public async Task<bool> StartFocusSession([FromBody]FocusSessionStartupOption option)
        {
            var session = await BreakSessionRepository.GetUnfinishedBreakSession(UserId).ConfigureAwait(false);

            if (session != null && !await BreakSessionService.StopBreakSession(UserId, session.Id).ConfigureAwait(false))
            {
                return false;
            }

            return await FocusSessionService.StartFocusSession(UserId, option).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("focus-session/{id}/stop")]
        public async Task<bool> StopFocusSession(string id)
        {
            return await FocusSessionService.StopFocusSession(UserId, id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("focus-session/work-items/{id}/start")]
        public async Task<bool> SwitchWorkItem(string id)
        {
            return await FocusSessionService.SwitchWorkItem(UserId, id).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("active-break-session")]
        public async Task<BreakSession> GetActiveBreakSession()
        {
            return await BreakSessionService.GetOpenBreakSession(UserId, false).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("stale-break-session")]
        public async Task<BreakSession> GetStaleBreakSession()
        {
            return await BreakSessionService.GetOpenBreakSession(UserId, true).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("break-session/start")]
        public async Task<bool> StartBreakSession([FromBody]BreakSessionStartupOption option)
        {
            return await BreakSessionService.StartBreakSession(UserId, option).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("break-session/{id}/stop")]
        public async Task<bool> StopBreakSession(string id)
        {
            return await BreakSessionService.StopBreakSession(UserId, id).ConfigureAwait(false);
        }
    }
}
