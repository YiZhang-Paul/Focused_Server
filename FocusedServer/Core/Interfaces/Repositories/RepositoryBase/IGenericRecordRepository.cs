using System.Threading.Tasks;

namespace Core.Interfaces.Repositories.RepositoryBase
{
    public interface IGenericRecordRepository<T>
    {
        Task<T> Get(string id);
        Task<T> Replace(T document);
    }
}
