using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories.RepositoryBase;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IWorkItemRepository : IUserOwnedRecordRepository<WorkItem>
    {
        Task<List<WorkItem>> GetWorkItems(string userId, WorkItemStatus status);
        Task<WorkItemDto> GetWorkItemMeta(string userId, string id);
        Task<List<WorkItemDto>> GetWorkItemMetas(string userId, List<string> ids, DateTime? start = null, DateTime? end = null);
        Task<List<WorkItemDto>> GetWorkItemMetas(string userId, WorkItemQuery query);
        Task<long> GetPastDueWorkItemsCount(string userId, DateTime start, DateTime end);
        Task<long> GetLoomingWorkItemsCount(string userId, DateTime start, DateTime end);
        Task<List<WorkItemProgressionDto>> GetWorkItemProgressionByDateRange(string userId, List<string> ids, DateTime start, DateTime end);
    }
}
