using Core.Models;
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

        public TimeSessionController(FocusSessionRepository focusSessionRepository)
        {
            FocusSessionRepository = focusSessionRepository;
        }

        [HttpGet]
        [Route("focus-session/{id}")]
        public async Task<FocusSession> GetFocusSession(string id)
        {
            return await FocusSessionRepository.Get(id).ConfigureAwait(false);
        }
    }
}
