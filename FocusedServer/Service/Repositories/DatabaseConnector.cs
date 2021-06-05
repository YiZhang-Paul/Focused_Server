using Core.Configurations;
using Core.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Service.Repositories
{
    public class DatabaseConnector<T> where T : DatabaseEntry
    {
        protected IMongoCollection<T> Collection { get; set; }

        public DatabaseConnector(IOptions<DatabaseConfiguration> configuration, string collection)
        {
            Collection = Connect(configuration.Value, collection);
        }

        private IMongoCollection<T> Connect(DatabaseConfiguration configuration, string collection)
        {
            var database = new MongoClient(configuration.Url).GetDatabase(configuration.Name);

            return database.GetCollection<T>(collection);
        }
    }
}
