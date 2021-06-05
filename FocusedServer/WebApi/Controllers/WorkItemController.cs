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

        [HttpGet]
        [Route("")]
        public async Task<List<WorkItemDto>> GetWorkItems()
        {
            return await WorkItemService.GetWorkItems().ConfigureAwait(false);
        }
    }
}
