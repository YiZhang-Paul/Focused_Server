using Core.Configurations;
using Core.Interfaces.Repositories;
using Core.Models.User;
using Microsoft.Extensions.Options;
using Service.Repositories.RepositoryBase;

namespace Service.Repositories
{
    public class UserProfileRepository : GenericRecordRepository<UserProfile>, IUserProfileRepository
    {
        public UserProfileRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(UserProfile).Name) { }
    }
}
