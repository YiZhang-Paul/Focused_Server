using Core.Interfaces.Repositories.RepositoryBase;
using Core.Models.User;

namespace Core.Interfaces.Repositories
{
    public interface IUserProfileRepository : IGenericRecordRepository<UserProfile>
    {
    }
}
