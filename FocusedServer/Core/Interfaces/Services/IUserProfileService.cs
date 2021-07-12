using Core.Models.User;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IUserProfileService
    {
        Task<PerformanceRating> UpdateUserRatings(string id, PerformanceRating ratings);
    }
}
