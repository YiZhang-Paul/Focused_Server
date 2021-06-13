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

        [HttpGet]
        [Route("{id}")]
        public async Task<WorkItem> GetWorkItem(string id)
        {
            return await WorkItemService.GetWorkItem(id).ConfigureAwait(false);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<WorkItem> UpdateWorkItem([FromBody]WorkItem item)
        {
            return await WorkItemService.UpdateWorkItem(item).ConfigureAwait(false);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<bool> DeleteWorkItem(string id)
        {
            return await WorkItemService.DeleteWorkItem(id).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{id}/meta")]
        public async Task<WorkItemDto> GetWorkItemMeta(string id)
        {
            return await WorkItemService.GetWorkItemMeta(id).ConfigureAwait(false);
        }

        [HttpPut]
        [Route("{id}/meta")]
        public async Task<WorkItemDto> UpdateWorkItemMeta([FromBody]WorkItemDto item, string id)
        {
            return await WorkItemService.UpdateWorkItemMeta(item, id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("summaries")]
        public async Task<List<WorkItemDto>> GetWorkItemSummaries([FromBody]WorkItemQuery query)
        {
            return await WorkItemService.GetWorkItemMetas(query).ConfigureAwait(false);
        }
    }
}
