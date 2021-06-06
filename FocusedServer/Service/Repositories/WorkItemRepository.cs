using Core.Configurations;
using Core.Dtos;
using Core.Enums;
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

        public async Task<List<WorkItemDto>> GetWorkItems(WorkItemQuery query)
        {
            return await Collection.Find(GetFilter(query))
                .Skip(query.Skip)
                .Limit(query.Limit)
                .Project(_ => new WorkItemDto
                {
                    Id = _.Id,
                    Name = _.Name,
                    Type = _.Type,
                    Priority = _.Priority,
                    Status = _.Status,
                    Estimation = _.Estimation,
                    SubtaskProgress = new ProgressionCounter<int>
                    {
                        Current = _.Subtasks.Count(task => task.Status == WorkItemStatus.Completed),
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

        private FilterDefinition<WorkItem> GetFilter(WorkItemQuery query)
        {
            var builder = Builders<WorkItem>.Filter;
            var filter = builder.Or(builder.Eq(_ => _.Parent, null), builder.Regex(_ => _.Parent, @"^\W*$"));

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                filter &= builder.Where(_ => _.Name.ToLower().Contains(query.SearchText.ToLower()));
            }

            if (query.IsHighlighted.HasValue)
            {
                if (query.IsHighlighted.Value)
                {
                    filter &= builder.Eq(_ => _.Status, WorkItemStatus.Highlighted);
                }
                else
                {
                    filter &= builder.Ne(_ => _.Status, WorkItemStatus.Highlighted);
                }
            }

            if (query.IsCompleted.HasValue)
            {
                if (query.IsCompleted.Value)
                {
                    filter &= builder.Eq(_ => _.Status, WorkItemStatus.Completed);
                }
                else
                {
                    filter &= builder.Ne(_ => _.Status, WorkItemStatus.Completed);
                }
            }

            if (query.Type.HasValue)
            {
                filter &= builder.Eq(_ => _.Type, query.Type);
            }

            return filter;
        }
    }
}
