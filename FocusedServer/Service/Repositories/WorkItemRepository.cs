using Core.Configurations;
using Core.Dtos;
using Core.Models.Generic;
using Core.Models.WorkItem;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class WorkItemRepository : DatabaseConnector<WorkItem>
    {
        public WorkItemRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(WorkItem).Name) { }

        public async Task<List<WorkItemDto>> GetWorkItems(int skip = 0, int limit = 0)
        {
            var filter = Builders<WorkItem>.Filter.Regex(_ => _.Parent, @"^\W*$");

            return await Collection.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .Project(_ => new WorkItemDto
                {
                    Name = _.Name,
                    Type = _.Type,
                    Priority = _.Priority,
                    Estimation = _.Estimation,
                    SubtaskProgress = new ProgressionCounter<int>
                    {
                        Current = _.Subtasks.Count(task => task.IsCompleted),
                        Target = _.Subtasks.Count
                    },
                    ChecklistProgress = new ProgressionCounter<int>
                    {
                        Current = _.Checklist.Count(entry => entry.IsCompleted),
                        Target = _.Checklist.Count
                    }
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
