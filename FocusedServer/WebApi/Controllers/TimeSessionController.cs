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
        private IFocusSessionService FocusSessionService { get; set; }

        public TimeSessionController(IBreakSessionRepository breakSessionRepository, IFocusSessionService focusSessionService)
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

        [HttpPost]
        [Route("focus-session")]
        public async Task<bool> StartFocusSession([FromBody]FocusSessionStartupOption option)
        {
            return await FocusSessionService.StartFocusSession(UserId, option).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("active-break-session")]
        public async Task<BreakSession> GetActiveBreakSession()
        {
            return await BreakSessionRepository.GetActiveBreakSession(UserId).ConfigureAwait(false);
        }
    }
}
