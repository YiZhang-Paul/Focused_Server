using Core.Configurations;
using Core.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Service.Repositories.RepositoryBase
{
    public class DatabaseConnector<T> where T : DatabaseEntry
    {
        protected IMongoCollection<T> Collection { get; set; }
        private IMongoDatabase Database { get; set; }

        public DatabaseConnector(IOptions<DatabaseConfiguration> configuration, string collection)
        {
            Database = new MongoClient(configuration.Value.Url).GetDatabase(configuration.Value.Name);
            Collection = Connect<T>(collection);
        }

        protected IMongoCollection<K> Connect<K>(string collection) where K : DatabaseEntry
        {
            return Database.GetCollection<K>(collection);
        }

        public async Task DropCollection()
        {
            await Database.DropCollectionAsync(typeof(T).Name).ConfigureAwait(false);
        }
    }
}
