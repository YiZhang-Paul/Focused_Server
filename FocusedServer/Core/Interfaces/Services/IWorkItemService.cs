using Core.Dtos;
using Core.Enums;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IWorkItemService
    {
        Task<string> CreateWorkItem(WorkItemDto item);
        Task<WorkItem> GetWorkItem(string userId, string id);
        Task<WorkItem> UpdateWorkItem(WorkItem item);
        Task<bool> StartWorkItem(string userId, string id);
        Task<bool> StopWorkItem(string userId, WorkItemStatus targetStatus = WorkItemStatus.Highlighted);
        Task<WorkItemDto> UpdateWorkItemMeta(WorkItemDto item);
        Task<ActivityBreakdownDto> GetWorkItemActivityBreakdownByDateRange(string userId, DateTime start, DateTime end);
        Task<List<WorkItemProgressionDto>> GetWorkItemCurrentProgressionByDateRange(string userId, DateTime start, DateTime end);
        Task<List<WorkItemProgressionDto>> GetWorkItemOverallProgressionByDateRange(string userId, DateTime start, DateTime end);
    }
}
