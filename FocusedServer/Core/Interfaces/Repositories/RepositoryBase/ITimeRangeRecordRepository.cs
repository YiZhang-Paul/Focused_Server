using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories.RepositoryBase
{
    public interface ITimeRangeRecordRepository<T> : IUserOwnedRecordRepository<T>
    {
        Task<List<T>> GetOpenTimeRange(string userId);
    }
}
