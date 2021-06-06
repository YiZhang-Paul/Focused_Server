using Core.Dtos;
using Core.Models.WorkItem;
using Service.Repositories;
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

        public async Task<bool> CreateWorkItem(WorkItemDto item)
        {
            try
            {
                var workItem = new WorkItem
                {
                    Name = item.Name.Trim(),
                    Type = item.Type,
                    Priority = item.Priority,
                    Estimation = item.Estimation
                };

                await WorkItemRepository.Add(workItem).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateWorkItemMeta(WorkItemDto item, string id)
        {
            var workItem = await WorkItemRepository.Get(id).ConfigureAwait(false);

            if (workItem == null)
            {
                return false;
            }

            workItem.Name = item.Name;
            workItem.Type = item.Type;
            workItem.Priority = item.Priority;
            workItem.Status = item.Status;
            workItem.Estimation = item.Estimation;

            return await WorkItemRepository.Replace(workItem).ConfigureAwait(false) != null;
        }

        public async Task<List<WorkItemDto>> GetWorkItems(WorkItemQuery query)
        {
            return await WorkItemRepository.GetWorkItems(query).ConfigureAwait(false);
        }
    }
}
