using Core.Interfaces.Repositories.RepositoryBase;
using Core.Models.TimeSession;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IFocusSessionRepository : ITimeRangeRecordRepository<FocusSession>
    {
        Task<FocusSession> GetUnfinishedFocusSession(string userId);
    }
}
