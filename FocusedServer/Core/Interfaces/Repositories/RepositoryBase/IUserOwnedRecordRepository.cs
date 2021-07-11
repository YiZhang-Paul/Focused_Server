using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories.RepositoryBase
{
    public interface IUserOwnedRecordRepository<T>
    {
        Task<T> Get(string userId, string id);
        Task<List<T>> Get(string userId, List<string> ids);
        Task<string> Add(T document);
        Task<List<string>> Add(List<T> documents);
        Task<T> Replace(T document);
        Task<bool> Delete(string userId, string id);
    }
}
