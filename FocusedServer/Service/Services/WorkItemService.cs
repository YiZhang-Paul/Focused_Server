using Core.Dtos;
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

        public async Task<List<WorkItemDto>> GetWorkItems(int skip = 0, int limit = 0)
        {
            return await WorkItemRepository.GetWorkItems(skip, limit).ConfigureAwait(false);
        }
    }
}
