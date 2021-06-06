using Core.Dtos;
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
        [Route("")]
        public async Task<List<WorkItemDto>> GetWorkItems()
        {
            return await WorkItemService.GetWorkItems().ConfigureAwait(false);
        }
    }
}
