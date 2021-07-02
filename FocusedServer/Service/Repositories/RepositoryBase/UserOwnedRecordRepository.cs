using Core.Configurations;
using Core.Interfaces.Repositories.RepositoryBase;
using Core.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories.RepositoryBase
{
    public class UserOwnedRecordRepository<T> : DatabaseConnector<T>, IUserOwnedRecordRepository<T> where T : UserOwnedRecord
    {
        public UserOwnedRecordRepository(IOptions<DatabaseConfiguration> configuration, string collection) : base(configuration, collection) { }

        public async Task<T> Get(string userId, string id)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Eq(_ => _.Id, id);

            return await Collection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<string> Add(T document)
        {
            if (string.IsNullOrWhiteSpace(document.UserId))
            {
                throw new ArgumentNullException("Must have a valid user Id.");
            }

            await Collection.InsertOneAsync(document).ConfigureAwait(false);

            return document.Id;
        }

        public async Task<List<string>> Add(List<T> documents)
        {
            if (documents.Any(_ => string.IsNullOrWhiteSpace(_.UserId)))
            {
                throw new ArgumentNullException("Must have a valid user Id.");
            }

            await Collection.InsertManyAsync(documents).ConfigureAwait(false);

            return documents.Select(_ => _.Id).ToList();
        }

        public async Task<T> Replace(T document)
        {
            if (string.IsNullOrWhiteSpace(document.UserId))
            {
                throw new ArgumentNullException("Must have a valid user Id.");
            }

            var builder = Builders<T>.Filter;
            var filter = builder.Eq(_ => _.UserId, document.UserId) & builder.Eq(_ => _.Id, document.Id);
            var result = await Collection.ReplaceOneAsync(filter, document).ConfigureAwait(false);

            return result.IsAcknowledged ? document : null;
        }

        public async Task<bool> Delete(string userId, string id)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Eq(_ => _.UserId, userId) & builder.Eq(_ => _.Id, id);

            return (await Collection.DeleteOneAsync(filter).ConfigureAwait(false)).IsAcknowledged;
        }
    }
}
