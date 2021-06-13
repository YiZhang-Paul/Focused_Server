using Core.Dtos;
using Core.Models.WorkItem;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Services
{
    public class WorkItemService
    {
        private WorkItemRepository WorkItemRepository { get; set; }

        public WorkItemService(WorkItemRepository workItemRepository)
        {
            WorkItemRepository = workItemRepository;
        }

        public async Task<string> CreateWorkItem(WorkItemDto item)
        {
            try
            {
                var workItem = new WorkItem
                {
                    Name = item.Name.Trim(),
                    Type = item.Type,
                    Priority = item.Priority,
                    EstimatedHours = item.ItemProgress.Target
                };

                return await WorkItemRepository.Add(workItem).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        public async Task<WorkItem> GetWorkItem(string id)
        {
            return await WorkItemRepository.Get(id).ConfigureAwait(false);
        }

        public async Task<WorkItem> UpdateWorkItem(WorkItem item)
        {
            item.TimeInfo.LastModified = DateTime.UtcNow;

            return await WorkItemRepository.Replace(item).ConfigureAwait(false);
        }

        public async Task<bool> DeleteWorkItem(string id)
        {
            return await WorkItemRepository.Delete(id).ConfigureAwait(false);
        }

        public async Task<WorkItemDto> UpdateWorkItemMeta(WorkItemDto item, string id)
        {
            var workItem = await WorkItemRepository.Get(id).ConfigureAwait(false);

            if (workItem == null)
            {
                return null;
            }

            workItem.Name = item.Name;
            workItem.Type = item.Type;
            workItem.Priority = item.Priority;
            workItem.Status = item.Status;
            workItem.EstimatedHours = item.ItemProgress.Target;

            if (await WorkItemRepository.Replace(workItem).ConfigureAwait(false) == null)
            {
                return null;
            }

            return await WorkItemRepository.GetWorkItemMeta(item.Id).ConfigureAwait(false);
        }

        public async Task<WorkItemDto> GetWorkItemMeta(string id)
        {
            return await WorkItemRepository.GetWorkItemMeta(id).ConfigureAwait(false);
        }

        public async Task<List<WorkItemDto>> GetWorkItemMetas(WorkItemQuery query)
        {
            return await WorkItemRepository.GetWorkItemMetas(query).ConfigureAwait(false);
        }
    }
}
