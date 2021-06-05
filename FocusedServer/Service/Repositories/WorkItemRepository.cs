using Core.Configurations;
using Core.Dtos;
using Core.Models.WorkItem;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class WorkItemRepository : DatabaseConnector<WorkItem>
    {
        public WorkItemRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(WorkItem).Name) { }

        public async Task<List<WorkItemDto>> GetWorkItems(int skip = 0, int limit = 0)
        {
            return await Collection.Find(new BsonDocument())
                .Skip(skip)
                .Limit(limit)
                .Project(_ => new WorkItemDto
                {
                    Name = _.Name,
                    Type = _.Type,
                    Priority = _.Priority,
                    Estimation = _.Estimation
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
