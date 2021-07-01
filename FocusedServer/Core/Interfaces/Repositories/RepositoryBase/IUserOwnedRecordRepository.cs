using System.Threading.Tasks;

namespace Core.Interfaces.Repositories.RepositoryBase
{
    public interface IUserOwnedRecordRepository<T>
    {
        Task<T> Get(string userId, string id);
        Task<string> Add(T document);
        Task<T> Replace(T document);
        Task<bool> Delete(string userId, string id);
    }
}
