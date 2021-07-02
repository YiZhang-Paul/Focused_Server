using System.Threading.Tasks;

namespace Core.Interfaces.Repositories.RepositoryBase
{
    public interface IGenericRecordRepository<T>
    {
        Task<T> Get(string id);
        Task<string> Add(T document);
        Task<T> Replace(T document);
        Task<bool> Delete(string id);
    }
}
