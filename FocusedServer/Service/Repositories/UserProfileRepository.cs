using Core.Configurations;
using Core.Models.User;
using Microsoft.Extensions.Options;

namespace Service.Repositories
{
    public class UserProfileRepository : GenericRecordRepository<UserProfile>
    {
        public UserProfileRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(UserProfile).Name) { }
    }
}
