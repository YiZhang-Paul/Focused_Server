using Core.Configurations;
using Core.Models;
using Microsoft.Extensions.Options;

namespace Service.Repositories
{
    public class FocusSessionRepository : DatabaseConnector<FocusSession>
    {
        public FocusSessionRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(FocusSession).Name) { }
    }
}
