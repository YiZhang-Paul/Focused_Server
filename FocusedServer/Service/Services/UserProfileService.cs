using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.User;
using System.Threading.Tasks;

namespace Service.Services
{
    public class UserProfileService : IUserProfileService
    {
        private IUserProfileRepository UserProfileRepository { get; set; }

        public UserProfileService(IUserProfileRepository userProfileRepository)
        {
            UserProfileRepository = userProfileRepository;
        }

        public async Task<PerformanceRating> UpdateUserRatings(string id, PerformanceRating ratings)
        {
            var user = await UserProfileRepository.Get(id).ConfigureAwait(false);

            if (user == null)
            {
                return null;
            }

            user.Ratings = ratings;

            return (await UserProfileRepository.Replace(user).ConfigureAwait(false))?.Ratings;
        }
    }
}
