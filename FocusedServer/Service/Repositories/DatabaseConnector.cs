using Core.Configurations;
using Core.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Service.Repositories
{
    public class DatabaseConnector<T> where T : DatabaseEntry
    {
        private DatabaseConfiguration Configuration { get; set; }
        protected IMongoCollection<T> Collection { get; set; }

        public DatabaseConnector(IOptions<DatabaseConfiguration> configuration, string collection)
        {
            Configuration = configuration.Value;
            Collection = Connect<T>(collection);
        }

        protected IMongoCollection<K> Connect<K>(string collection) where K : DatabaseEntry
        {
            return new MongoClient(Configuration.Url).GetDatabase(Configuration.Name).GetCollection<K>(collection);
        }
    }
}
