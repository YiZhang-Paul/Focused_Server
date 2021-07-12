using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
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
        private IUserProfileService UserProfileService { get; set; }

        public UserProfileController(IUserProfileRepository userProfileRepository, IUserProfileService userProfileService)
        {
            UserProfileRepository = userProfileRepository;
            UserProfileService = userProfileService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<UserProfile> GetUserProfile(string id)
        {
            return await UserProfileRepository.Get(id).ConfigureAwait(false);
        }

        [HttpPut]
        [Route("{id}/ratings")]
        public async Task<PerformanceRating> UpdateUserRatings(string id, PerformanceRating ratings)
        {
            return await UserProfileService.UpdateUserRatings(id, ratings).ConfigureAwait(false);
        }
    }
}
