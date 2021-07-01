using Core.Dtos;
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
        Task<bool> DeleteWorkItem(string userId, string id);
        Task<bool> StartWorkItem(string userId, string id);
        Task<bool> StopWorkItem(string userId);
        Task<WorkItemDto> GetWorkItemMeta(string userId, string id);
        Task<List<WorkItemDto>> GetWorkItemMetas(string userId, WorkItemQuery query);
        Task<WorkItemDto> UpdateWorkItemMeta(WorkItemDto item);
        Task<ActivityBreakdownDto> GetWorkItemActivityBreakdownByDateRange(string userId, DateTime start, DateTime end);
        Task<List<WorkItemProgressionDto>> GetWorkItemProgressionByDateRange(string userId, DateTime start, DateTime end);
    }
}
