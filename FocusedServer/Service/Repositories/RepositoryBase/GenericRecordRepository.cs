using Core.Configurations;
using Core.Interfaces.Repositories.RepositoryBase;
using Core.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Service.Repositories.RepositoryBase
{
    public class GenericRecordRepository<T> : DatabaseConnector<T>, IGenericRecordRepository<T> where T : DatabaseEntry
    {
        public GenericRecordRepository(IOptions<DatabaseConfiguration> configuration, string collection) : base(configuration, collection) { }

        public async Task<T> Get(string id)
        {
            var filter = Builders<T>.Filter.Eq(_ => _.Id, id);

            return await Collection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<string> Add(T document)
        {
            await Collection.InsertOneAsync(document).ConfigureAwait(false);

            return document.Id;
        }

        public async Task<T> Replace(T document)
        {
            var filter = Builders<T>.Filter.Eq(_ => _.Id, document.Id);
            var result = await Collection.ReplaceOneAsync(filter, document).ConfigureAwait(false);

            return result.IsAcknowledged && result.MatchedCount == 1 ? document : null;
        }
    }
}
