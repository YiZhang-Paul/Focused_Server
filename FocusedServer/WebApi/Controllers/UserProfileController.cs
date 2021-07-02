using Core.Interfaces.Repositories;
using Core.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/user-profile")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private IUserProfileRepository UserProfileRepository { get; set; }

        public UserProfileController(IUserProfileRepository userProfileRepository)
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
