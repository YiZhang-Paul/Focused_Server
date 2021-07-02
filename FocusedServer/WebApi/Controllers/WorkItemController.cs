using Core.Dtos;
using Core.Interfaces.Services;
using Core.Models.WorkItem;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/work-items")]
    [ApiController]
    public class WorkItemController : ControllerBase
    {
        private const string UserId = "60cd1862629e063c384f3ea1";
        private IWorkItemService WorkItemService { get; set; }

        public WorkItemController(IWorkItemService workItemService)
        {
            WorkItemService = workItemService;
        }

        [HttpPost]
        [Route("")]
        public async Task<string> CreateWorkItem([FromBody]WorkItemDto item)
        {
            item.UserId = UserId;

            return await WorkItemService.CreateWorkItem(item).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<WorkItem> GetWorkItem(string id)
        {
            return await WorkItemService.GetWorkItem(UserId, id).ConfigureAwait(false);
        }

        [HttpPut]
        [Route("")]
        public async Task<WorkItem> UpdateWorkItem([FromBody]WorkItem item)
        {
            return await WorkItemService.UpdateWorkItem(item).ConfigureAwait(false);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<bool> DeleteWorkItem(string id)
        {
            return await WorkItemService.DeleteWorkItem(UserId, id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("{id}/start")]
        public async Task<bool> StartWorkItem(string id)
        {
            return await WorkItemService.StartWorkItem(UserId, id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("stop")]
        public async Task<bool> StopWorkItem()
        {
            return await WorkItemService.StopWorkItem(UserId).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{id}/meta")]
        public async Task<WorkItemDto> GetWorkItemMeta(string id)
        {
            return await WorkItemService.GetWorkItemMeta(UserId, id).ConfigureAwait(false);
        }

        [HttpPut]
        [Route("meta")]
        public async Task<WorkItemDto> UpdateWorkItemMeta([FromBody]WorkItemDto item)
        {
            return await WorkItemService.UpdateWorkItemMeta(item).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("summaries")]
        public async Task<List<WorkItemDto>> GetWorkItemSummaries([FromBody]WorkItemQuery query)
        {
            return await WorkItemService.GetWorkItemMetas(UserId, query).ConfigureAwait(false);
        }
    }
}
