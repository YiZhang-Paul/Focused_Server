using Core.Dtos;
using Core.Models.WorkItem;
using Microsoft.AspNetCore.Mvc;
using Service.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/work-items")]
    [ApiController]
    public class WorkItemController : ControllerBase
    {
        private WorkItemService WorkItemService { get; set; }

        public WorkItemController(WorkItemService workItemService)
        {
            WorkItemService = workItemService;
        }

        [HttpPost]
        [Route("")]
        public async Task<bool> CreateWorkItem([FromBody]WorkItemDto item)
        {
            return await WorkItemService.CreateWorkItem(item).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("summaries")]
        public async Task<List<WorkItemDto>> GetWorkItemSummaries([FromBody]WorkItemQuery query)
        {
            return await WorkItemService.GetWorkItems(query).ConfigureAwait(false);
        }
    }
}
