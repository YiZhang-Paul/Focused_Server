using Core.Models.User;
using Microsoft.AspNetCore.Mvc;
using Service.Repositories;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/user-profile")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private UserProfileRepository UserProfileRepository { get; set; }

        public UserProfileController(UserProfileRepository userProfileRepository)
        {
            UserProfileRepository = userProfileRepository;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<UserProfile> GetUserProfile(string id)
        {
            return await UserProfileRepository.Get(id).ConfigureAwait(false);
        }
    }
}
