using Core.Models.TimeSession;
using Microsoft.AspNetCore.Mvc;
using Service.Repositories;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/time-session")]
    [ApiController]
    public class TimeSessionController : ControllerBase
    {
        private FocusSessionRepository FocusSessionRepository { get; set; }
        private BreakSessionRepository BreakSessionRepository { get; set; }

        public TimeSessionController(FocusSessionRepository focusSessionRepository, BreakSessionRepository breakSessionRepository)
        {
            FocusSessionRepository = focusSessionRepository;
            BreakSessionRepository = breakSessionRepository;
        }

        [HttpGet]
        [Route("focus-session/{id}")]
        public async Task<FocusSession> GetFocusSession(string id)
        {
            return await FocusSessionRepository.Get(id).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("break-session/{id}")]
        public async Task<BreakSession> GetBreakSession(string id)
        {
            return await BreakSessionRepository.Get(id).ConfigureAwait(false);
        }
    }
}
